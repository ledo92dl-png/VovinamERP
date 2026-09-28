using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.CreateReceipt;

public sealed record CreateReceiptResult(
    Guid ReceiptId,
    Guid TenantId,
    Guid CollectedByUserId,
    string ReceiptNumber,
    PaymentMethod PaymentMethod,
    DateOnly ReceiptDate,
    decimal Amount,
    ReceiptStatus Status,
    string? TransactionReference,
    string? EvidenceImageUrl,
    string? Note);