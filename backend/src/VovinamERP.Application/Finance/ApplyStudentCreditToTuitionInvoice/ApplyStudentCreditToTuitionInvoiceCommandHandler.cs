using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;

public sealed class ApplyStudentCreditToTuitionInvoiceCommandHandler
    : IRequestHandler<
        ApplyStudentCreditToTuitionInvoiceCommand,
        ApplyStudentCreditToTuitionInvoiceResult>
{
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
    private readonly IStudentCreditRepository _studentCreditRepository;
    private readonly IStudentCreditTransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyStudentCreditToTuitionInvoiceCommandHandler(
        ITuitionInvoiceRepository tuitionInvoiceRepository,
        IStudentCreditRepository studentCreditRepository,
        IStudentCreditTransaction transaction,
        IUnitOfWork unitOfWork)
    {
        _tuitionInvoiceRepository = tuitionInvoiceRepository;
        _studentCreditRepository = studentCreditRepository;
        _transaction = transaction;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplyStudentCreditToTuitionInvoiceResult> Handle(
        ApplyStudentCreditToTuitionInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "TenantId is required.");
        }

        if (request.StudentId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "StudentId is required.");
        }

        if (request.TuitionInvoiceId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "TuitionInvoiceId is required.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Credit amount must be greater than zero.");
        }

        var transactionResult =
            await _transaction.ExecuteWithStudentAndInvoiceLockAsync(
                request.TenantId,
                request.StudentId,
                request.TuitionInvoiceId,
                async ct =>
                {
                    // Reload the invoice after acquiring the locks.
                    var invoice =
                        await _tuitionInvoiceRepository.GetByIdAsync(
                            request.TenantId,
                            request.TuitionInvoiceId,
                            ct);

                    if (invoice is null)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(
                                new Error(
                                    "FIN_CREDIT_001",
                                    "Tuition invoice was not found."));
                    }

                    if (invoice.StudentId != request.StudentId)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(
                                new Error(
                                    "FIN_CREDIT_002",
                                    "Student does not match the tuition invoice."));
                    }

                    // Read the balance only after the student lock is acquired.
                    var availableCredit =
                        await _studentCreditRepository.GetBalanceAsync(
                            request.TenantId,
                            request.StudentId,
                            ct);

                    if (availableCredit < request.Amount)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(
                                new Error(
                                    "FIN_CREDIT_003",
                                    $"Insufficient student credit. " +
                                    $"Available: {availableCredit}, " +
                                    $"requested: {request.Amount}."));
                    }

                    if (request.Amount > invoice.BalanceAmount)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(
                                new Error(
                                    "FIN_CREDIT_004",
                                    $"Credit amount exceeds invoice balance. " +
                                    $"Invoice balance: {invoice.BalanceAmount}, " +
                                    $"requested: {request.Amount}."));
                    }

                    var applyResult =
                        invoice.ApplyCredit(request.Amount);

                    if (applyResult.IsFailure)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(applyResult.Error);
                    }

                    var debitResult =
                        StudentCreditTransaction.Create(
                            request.TenantId,
                            request.StudentId,
                            StudentCreditTransactionType.Debit,
                            request.Amount,
                            request.TransactionDate,
                            null,
                            null,
                            invoice.Id,
                            request.Note ??
                                $"Apply student credit to tuition " +
                                $"{invoice.Month:D2}/{invoice.Year}");

                    if (debitResult.IsFailure ||
                        debitResult.Value is null)
                    {
                        return Result<ApplyStudentCreditToTuitionInvoiceResult>
                            .Failure(debitResult.Error);
                    }

                    await _studentCreditRepository.AddAsync(
                        debitResult.Value,
                        ct);

                    _tuitionInvoiceRepository.Update(invoice);

                    await _unitOfWork.SaveChangesAsync(ct);

                    var remainingCredit =
                        availableCredit - request.Amount;

                    var result =
                        new ApplyStudentCreditToTuitionInvoiceResult(
                            invoice.Id,
                            invoice.StudentId,
                            request.Amount,
                            remainingCredit,
                            invoice.PaidAmount,
                            invoice.BalanceAmount,
                            invoice.Status.ToString());

                    return Result<ApplyStudentCreditToTuitionInvoiceResult>
                        .Success(result);
                },
                cancellationToken);

        if (transactionResult.IsFailure ||
            transactionResult.Value is null)
        {
            throw new InvalidOperationException(
                transactionResult.Error.Message);
        }

        return transactionResult.Value;
    }
}