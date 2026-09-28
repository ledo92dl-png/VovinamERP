using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.ConfirmTuitionRefund;

public sealed record ConfirmTuitionRefundResult(
    Guid TuitionRefundId,
    Guid TuitionAdjustmentId,
    Guid TuitionInvoiceId,
    Guid StudentId,
    decimal Amount,
    TuitionRefundStatus Status,
    DateOnly RefundedDate,
    Guid RefundedByUserId,
    PaymentMethod PaymentMethod,
    string? TransactionReference,
    string? EvidenceImageUrl
);