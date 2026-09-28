using VovinamERP.Domain.Finance;

namespace VovinamERP.Api.Controllers.Receipts;

public sealed record CreateReceiptRequest(
    Guid TenantId,
    Guid CollectedByUserId,
    string ReceiptNumber,
    PaymentMethod PaymentMethod,
    DateOnly ReceiptDate,
    string? TransactionReference,
    string? EvidenceImageUrl,
    string? Note);