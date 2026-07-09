using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class Receipt : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TuitionInvoiceId { get; private set; }
    public Guid TuitionPaymentId { get; private set; }
    public string ReceiptNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public DateOnly ReceiptDate { get; private set; }
    public ReceiptStatus Status { get; private set; }
    public string? Note { get; private set; }

    private Receipt() { }

    private Receipt(
        Guid tenantId,
        Guid tuitionInvoiceId,
        Guid tuitionPaymentId,
        string receiptNumber,
        decimal amount,
        DateOnly receiptDate,
        string? note)
    {
        TenantId = tenantId;
        TuitionInvoiceId = tuitionInvoiceId;
        TuitionPaymentId = tuitionPaymentId;
        ReceiptNumber = receiptNumber.Trim();
        Amount = amount;
        ReceiptDate = receiptDate;
        Status = ReceiptStatus.Draft;
        Note = note?.Trim();
    }

    public static Result<Receipt> Create(
        Guid tenantId,
        Guid tuitionInvoiceId,
        Guid tuitionPaymentId,
        string receiptNumber,
        decimal amount,
        DateOnly receiptDate,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<Receipt>.Failure(FinanceErrors.TenantRequired);

        if (amount <= 0)
            return Result<Receipt>.Failure(FinanceErrors.AmountMustBePositive);

        return Result<Receipt>.Success(
            new Receipt(tenantId, tuitionInvoiceId, tuitionPaymentId, receiptNumber, amount, receiptDate, note));
    }

    public Result Confirm()
    {
        if (Status == ReceiptStatus.Confirmed)
            return Result.Success();

        Status = ReceiptStatus.Confirmed;
        RaiseDomainEvent(new ReceiptConfirmedEvent(Id, ReceiptNumber));
        return Result.Success();
    }

    public Result UpdateNote(string? note)
    {
        if (Status == ReceiptStatus.Confirmed)
            return Result.Failure(FinanceErrors.ConfirmedReceiptCannotBeEdited);

        Note = note?.Trim();
        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = ReceiptStatus.Archived;
        base.Archive(userId);
    }
}
