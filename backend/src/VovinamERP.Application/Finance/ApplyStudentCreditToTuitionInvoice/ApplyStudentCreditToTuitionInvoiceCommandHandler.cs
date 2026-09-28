using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;

public sealed class ApplyStudentCreditToTuitionInvoiceCommandHandler
    : IRequestHandler<
        ApplyStudentCreditToTuitionInvoiceCommand,
        ApplyStudentCreditToTuitionInvoiceResult>
{
    private readonly ITuitionInvoiceRepository _tuitionInvoiceRepository;
    private readonly IStudentCreditRepository _studentCreditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyStudentCreditToTuitionInvoiceCommandHandler(
        ITuitionInvoiceRepository tuitionInvoiceRepository,
        IStudentCreditRepository studentCreditRepository,
        IUnitOfWork unitOfWork)
    {
        _tuitionInvoiceRepository = tuitionInvoiceRepository;
        _studentCreditRepository = studentCreditRepository;
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

        var availableCredit =
            await _studentCreditRepository.GetBalanceAsync(
                request.TenantId,
                request.StudentId,
                cancellationToken);

        if (availableCredit < request.Amount)
        {
            throw new InvalidOperationException(
                $"Insufficient student credit. " +
                $"Available: {availableCredit}, requested: {request.Amount}.");
        }

        if (request.Amount > invoice.BalanceAmount)
        {
            throw new InvalidOperationException(
                $"Credit amount exceeds invoice balance. " +
                $"Invoice balance: {invoice.BalanceAmount}, " +
                $"requested: {request.Amount}.");
        }

        var applyResult = invoice.ApplyCredit(
            request.Amount);

        if (applyResult.IsFailure)
        {
            throw new InvalidOperationException(
                applyResult.Error.Message);
        }

        var debitResult = StudentCreditTransaction.Create(
            request.TenantId,
            request.StudentId,
            StudentCreditTransactionType.Debit,
            request.Amount,
            request.TransactionDate,
            null,
            null,
            invoice.Id,
            request.Note ??
                $"Áp dụng số dư có vào học phí {invoice.Month:D2}/{invoice.Year}");

        if (debitResult.IsFailure ||
            debitResult.Value is null)
        {
            throw new InvalidOperationException(
                debitResult.Error.Message);
        }

        await _studentCreditRepository.AddAsync(
            debitResult.Value,
            cancellationToken);

        _tuitionInvoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var remainingCredit =
            availableCredit - request.Amount;

        return new ApplyStudentCreditToTuitionInvoiceResult(
            invoice.Id,
            invoice.StudentId,
            request.Amount,
            remainingCredit,
            invoice.PaidAmount,
            invoice.BalanceAmount,
            invoice.Status.ToString());
    }
}