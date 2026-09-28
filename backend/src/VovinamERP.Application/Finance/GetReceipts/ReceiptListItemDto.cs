namespace VovinamERP.Application.Finance.GetReceipts;

public sealed record ReceiptListItemDto(
    Guid Id,
    string ReceiptNumber,
    DateOnly ReceiptDate,
    Guid CollectedByUserId,
    string PaymentMethod,
    decimal Amount,
    string Status,
    int ItemCount,
    string? Note);