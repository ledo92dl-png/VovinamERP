namespace VovinamERP.Application.Finance.GetReceiptById;

public sealed record ReceiptDetailsDto(
    Guid Id,
    Guid TenantId,
    Guid CollectedByUserId,
    string ReceiptNumber,
    DateOnly ReceiptDate,
    string PaymentMethod,
    decimal Amount,
    string Status,
    string? TransactionReference,
    string? EvidenceImageUrl,
    string? Note,
    IReadOnlyCollection<ReceiptItemDto> Items);