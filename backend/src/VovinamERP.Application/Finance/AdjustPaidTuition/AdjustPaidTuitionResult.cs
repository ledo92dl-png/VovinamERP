
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.AdjustPaidTuition;

public sealed record AdjustPaidTuitionResult(
    Guid TuitionAdjustmentId,
    Guid TuitionInvoiceId,
    Guid StudentId,
    decimal PreviousPayableAmount,
    decimal NewPayableAmount,
    decimal AdjustmentAmount,
    decimal SettlementAmount,
    decimal RemainingBalance,
    TuitionAdjustmentSettlementType SettlementType,
    Guid? StudentCreditTransactionId,
    Guid? TuitionRefundId,
    TuitionRefundStatus? RefundStatus
);