using FluentValidation.Results;
using MediatR;
using VovinamERP.Application.Common.Exceptions;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;

namespace VovinamERP.Application.Finance.ApplySpecialTuitionDiscount;

public sealed class ApplySpecialTuitionDiscountCommandHandler
    : IRequestHandler<
        ApplySpecialTuitionDiscountCommand,
        ApplySpecialTuitionDiscountResult>
{
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplySpecialTuitionDiscountCommandHandler(
        ITuitionInvoiceRepository tuitionInvoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _tuitionInvoiceRepository = tuitionInvoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplySpecialTuitionDiscountResult> Handle(
        ApplySpecialTuitionDiscountCommand request,
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

        
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "Special discount reason is required.");
        }

        if (request.ApprovedByUserId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Special discount approver is required.");
        }

        var invoice =
            await _tuitionInvoiceRepository.GetByIdAsync(
                request.TenantId,
                request.TuitionInvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            throw new InvalidOperationException(
                $"Tuition invoice '{request.TuitionInvoiceId}' was not found.");
        }

        if (invoice.StudentId != request.StudentId)
        {
            throw new InvalidOperationException(
                "Student does not match the tuition invoice.");
        }

        var applyResult =
    invoice.ApplySpecialDiscount(
        request.DiscountType,
        request.DiscountValue,
        request.Reason,
        request.ApprovedByUserId);

        if (applyResult.IsFailure)
{
    throw new ValidationException(
        new[]
        {
            new ValidationFailure(
                "SpecialDiscount",
                $"{applyResult.Error.Code} - {applyResult.Error.Message}")
        });
}

        _tuitionInvoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var finalPayableAmount =
            invoice.Amount
            - invoice.DiscountAmount
            - invoice.SpecialDiscountAmount;

        return new ApplySpecialTuitionDiscountResult(
    invoice.Id,
    invoice.StudentId,
    invoice.Amount,
    invoice.DiscountAmount,
    invoice.SpecialDiscountType!.Value,
    invoice.SpecialDiscountValue!.Value,
    invoice.SpecialDiscountAmount,
    finalPayableAmount,
    invoice.PaidAmount,
    invoice.BalanceAmount,
    invoice.Status,
    invoice.SpecialDiscountReason!,
    invoice.SpecialDiscountApprovedByUserId!.Value,
    invoice.SpecialDiscountApprovedAtUtc!.Value);
    }
}