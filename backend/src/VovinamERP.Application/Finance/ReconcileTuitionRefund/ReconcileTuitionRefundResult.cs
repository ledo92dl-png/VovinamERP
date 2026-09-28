using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.ReconcileTuitionRefund;

public sealed record ReconcileTuitionRefundResult(
    Guid TuitionRefundId,
    Guid TuitionAdjustmentId,
    Guid TuitionInvoiceId,
    Guid StudentId,
    decimal Amount,
    TuitionRefundStatus Status,
    Guid ReconciledByUserId,
    DateTime ReconciledAtUtc
);