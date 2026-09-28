using MediatR;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.CreateReceipt;

public sealed record CreateReceiptCommand(
    Guid TenantId,
    Guid CollectedByUserId,
    string ReceiptNumber,
    PaymentMethod PaymentMethod,
    DateOnly ReceiptDate,
    string? TransactionReference,
    string? EvidenceImageUrl,
    string? Note)
    : IRequest<CreateReceiptResult>;