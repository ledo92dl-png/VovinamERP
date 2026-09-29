using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ConfirmReceipt;

public sealed class ConfirmReceiptCommandHandler
    : IRequestHandler<ConfirmReceiptCommand, Result>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
    private readonly IStudentCreditRepository _studentCreditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReceiptTransaction _receiptTransaction;

    public ConfirmReceiptCommandHandler(
        IReceiptRepository receiptRepository,
        ITuitionInvoiceRepository tuitionInvoiceRepository,
        IStudentCreditRepository studentCreditRepository,
        IUnitOfWork unitOfWork,
        IReceiptTransaction receiptTransaction)
    {
        _receiptRepository = receiptRepository;
        _tuitionInvoiceRepository = tuitionInvoiceRepository;
        _studentCreditRepository = studentCreditRepository;
        _unitOfWork = unitOfWork;
        _receiptTransaction = receiptTransaction;
    }

    public Task<Result> Handle(
        ConfirmReceiptCommand request,
        CancellationToken cancellationToken)
    {
        return _receiptTransaction.ExecuteWithReceiptLockAsync(
            request.TenantId,
            request.ReceiptId,
            async ct =>
            {
                var receipt =
                    await _receiptRepository.GetByIdAsync(
                        request.ReceiptId,
                        ct);

                if (receipt is null)
                {
                    return Result.Failure(
                        new Error(
                            "FIN_040",
                            "Receipt was not found."));
                }

                if (receipt.TenantId != request.TenantId)
                {
                    return Result.Failure(
                        new Error(
                            "FIN_041",
                            "Receipt does not belong to the specified tenant."));
                }

                // An already confirmed receipt is idempotent.
                // Do not create its payment or credit a second time.
                if (receipt.Status == ReceiptStatus.Confirmed)
                {
                    return Result.Success();
                }

                var confirmResult = receipt.Confirm();

                if (confirmResult.IsFailure)
                {
                    return confirmResult;
                }
                                var tuitionInvoiceIds =
                    receipt.Items
                        .Where(x =>
                            x.ItemType == ReceiptItemType.Tuition &&
                            x.ReferenceId.HasValue &&
                            x.ReferenceId.Value != Guid.Empty)
                        .Select(x => x.ReferenceId!.Value)
                        .Distinct()
                        .ToArray();

                await _receiptTransaction.LockTuitionInvoicesAsync(
                    request.TenantId,
                    tuitionInvoiceIds,
                    ct);

                foreach (var item in receipt.Items)
                {
                    if (item.ItemType != ReceiptItemType.Tuition)
                    {
                        continue;
                    }

                    if (!item.ReferenceId.HasValue ||
                        item.ReferenceId.Value == Guid.Empty)
                    {
                        return Result.Failure(
                            new Error(
                                "FIN_043",
                                "Tuition receipt item must reference a tuition invoice."));
                    }

                    var tuitionInvoice =
                        await _tuitionInvoiceRepository.GetByIdAsync(
                            request.TenantId,
                            item.ReferenceId.Value,
                            ct);

                    if (tuitionInvoice is null)
                    {
                        return Result.Failure(
                            new Error(
                                "FIN_044",
                                "Referenced tuition invoice was not found."));
                    }

                    if (!item.StudentId.HasValue ||
                        item.StudentId.Value != tuitionInvoice.StudentId)
                    {
                        return Result.Failure(
                            new Error(
                                "FIN_045",
                                "Receipt item student does not match the tuition invoice student."));
                    }

                    var paymentMethodResult =
                        MapPaymentMethod(receipt.PaymentMethod);

                    if (paymentMethodResult.IsFailure)
                    {
                        return Result.Failure(
                            paymentMethodResult.Error);
                    }

                    var balanceBeforePayment =
                        tuitionInvoice.BalanceAmount;

                    var appliedToInvoice =
                        Math.Min(
                            item.TotalAmount,
                            balanceBeforePayment);

                    var excessAmount =
                        item.TotalAmount - appliedToInvoice;

                    if (appliedToInvoice > 0)
                    {
                        var paymentResult =
                            tuitionInvoice.RecordPayment(
                                item.Id,
                                receipt.ReceiptNumber,
                                appliedToInvoice,
                                paymentMethodResult.Value,
                                receipt.ReceiptDate,
                                item.Note);

                        if (paymentResult.IsFailure)
                        {
                            return Result.Failure(
                                paymentResult.Error);
                        }
                    }

                    if (excessAmount > 0)
                    {
                        var creditResult =
                            StudentCreditTransaction.Create(
                                request.TenantId,
                                tuitionInvoice.StudentId,
                                StudentCreditTransactionType.Credit,
                                excessAmount,
                                receipt.ReceiptDate,
                                receipt.Id,
                                item.Id,
                                tuitionInvoice.Id,
                                $"Overpayment from receipt {receipt.ReceiptNumber}");

                        if (creditResult.IsFailure ||
                            creditResult.Value is null)
                        {
                            return Result.Failure(
                                creditResult.Error);
                        }

                        await _studentCreditRepository.AddAsync(
                            creditResult.Value,
                            ct);
                    }
                }

                await _unitOfWork.SaveChangesAsync(ct);

                return Result.Success();
            },
            cancellationToken);
    }

    private static Result<TuitionPaymentMethod> MapPaymentMethod(
        PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.Cash =>
                Result<TuitionPaymentMethod>.Success(
                    TuitionPaymentMethod.Cash),

            PaymentMethod.BankTransfer =>
                Result<TuitionPaymentMethod>.Success(
                    TuitionPaymentMethod.BankTransfer),

            PaymentMethod.EWallet =>
                Result<TuitionPaymentMethod>.Success(
                    TuitionPaymentMethod.Other),

            PaymentMethod.Other =>
                Result<TuitionPaymentMethod>.Success(
                    TuitionPaymentMethod.Other),

            _ =>
                Result<TuitionPaymentMethod>.Failure(
                    new Error(
                        "FIN_046",
                        "Unsupported receipt payment method."))
        };
    }
}