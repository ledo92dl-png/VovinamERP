using System.Linq;
using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class Receipt : AggregateRoot
{
    private readonly List<ReceiptItem> _items = [];

    public Guid TenantId { get; private set; }
    public string ReceiptNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public IReadOnlyCollection<ReceiptItem> Items => _items.AsReadOnly();
    public DateOnly ReceiptDate { get; private set; }
    public Guid CollectedByUserId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? EvidenceImageUrl { get; private set; }
    public ReceiptStatus Status { get; private set; }
    public string? Note { get; private set; }

    private Receipt()
    {
    }

    private Receipt(
    Guid tenantId,
    Guid collectedByUserId,
    string receiptNumber,
    PaymentMethod paymentMethod,
    DateOnly receiptDate,
    string? transactionReference,
    string? evidenceImageUrl,
    string? note)
    {
        TenantId = tenantId;

CollectedByUserId = collectedByUserId;

ReceiptNumber = receiptNumber.Trim();

PaymentMethod = paymentMethod;

TransactionReference = transactionReference?.Trim();

EvidenceImageUrl = evidenceImageUrl?.Trim();

ReceiptDate = receiptDate;

Amount = 0;

Status = ReceiptStatus.Draft;

Note = note?.Trim();
    }

    public static Result<Receipt> Create(
    Guid tenantId,
    Guid collectedByUserId,
    string receiptNumber,
    PaymentMethod paymentMethod,
    DateOnly receiptDate,
    string? transactionReference,
    string? evidenceImageUrl,
    string? note)
    {
        if (tenantId == Guid.Empty)
        {
            return Result<Receipt>.Failure(
                FinanceErrors.TenantRequired);
        }

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            return Result<Receipt>.Failure(
                new Error(
                    "FIN_015",
                    "Receipt number is required."));
        }
        if (collectedByUserId == Guid.Empty)
{
    return Result<Receipt>.Failure(
        new Error(
            "FIN_016",
            "Collector is required."));
}
        return Result<Receipt>.Success(
    new Receipt(
        tenantId,
        collectedByUserId,
        receiptNumber,
        paymentMethod,
        receiptDate,
        transactionReference,
        evidenceImageUrl,
        note));
    }

    public Result Confirm()
{
    if (Status == ReceiptStatus.Confirmed)
    {
        return Result.Success();
    }

    if (Status == ReceiptStatus.Archived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (_items.Count == 0)
    {
        return Result.Failure(
            new Error(
                "FIN_024",
                "Receipt must contain at least one item."));
    }

    RecalculateAmount();

    if (Amount <= 0)
    {
        return Result.Failure(
            FinanceErrors.AmountMustBePositive);
    }

    Status = ReceiptStatus.Confirmed;

    RaiseDomainEvent(
        new ReceiptConfirmedEvent(
            Id,
            ReceiptNumber));

    return Result.Success();
}

    public Result UpdateNote(string? note)
    {
        if (Status == ReceiptStatus.Confirmed)
        {
            return Result.Failure(
                FinanceErrors.ConfirmedReceiptCannotBeEdited);
        }

        Note = note?.Trim();

        return Result.Success();
    }

    public Result AddItem(ReceiptItem item)
{
    if (Status == ReceiptStatus.Confirmed)
    {
        return Result.Failure(
            FinanceErrors.ConfirmedReceiptCannotBeEdited);
    }

    if (Status == ReceiptStatus.Archived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (item is null)
    {
        return Result.Failure(
            new Error(
                "FIN_020",
                "Receipt item is required."));
    }

    if (item.TenantId != TenantId)
    {
        return Result.Failure(
            new Error(
                "FIN_022",
                "Receipt item belongs to another tenant."));
    }

    if (item.ReceiptId != Id)
    {
        return Result.Failure(
            new Error(
                "FIN_023",
                "Receipt item belongs to another receipt."));
    }

    if (_items.Any(x => x.Id == item.Id))
    {
        return Result.Success();
    }

    _items.Add(item);

    RecalculateAmount();

    return Result.Success();
}

   public Result RemoveItem(Guid receiptItemId)
{
    if (Status == ReceiptStatus.Confirmed)
    {
        return Result.Failure(
            FinanceErrors.ConfirmedReceiptCannotBeEdited);
    }

    if (Status == ReceiptStatus.Archived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (receiptItemId == Guid.Empty)
    {
        return Result.Failure(
            new Error(
                "FIN_021",
                "Receipt item not found."));
    }

    var item = _items.FirstOrDefault(
        x => x.Id == receiptItemId);

    if (item is null)
    {
        return Result.Failure(
            new Error(
                "FIN_021",
                "Receipt item not found."));
    }

    _items.Remove(item);

    RecalculateAmount();

    return Result.Success();
}

    public override void Archive(Guid? userId)
    {
        if (IsArchived)
        {
            return;
        }

        Status = ReceiptStatus.Archived;

        base.Archive(userId);
    }

    private void RecalculateAmount()
    {
        Amount = _items.Sum(
            x => x.TotalAmount);
    }
}