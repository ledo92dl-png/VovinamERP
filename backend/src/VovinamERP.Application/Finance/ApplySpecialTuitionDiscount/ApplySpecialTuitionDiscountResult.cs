using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.ApplySpecialTuitionDiscount;

public sealed record ApplySpecialTuitionDiscountResult(
    Guid TuitionInvoiceId,
    Guid StudentId,
    decimal StandardAmount,
    decimal AttendanceDiscountAmount,
    TuitionSpecialDiscountType SpecialDiscountType,
    decimal SpecialDiscountValue,
    decimal SpecialDiscountAmount,
    decimal FinalPayableAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    TuitionInvoiceStatus Status,
    string SpecialDiscountReason,
    Guid SpecialDiscountApprovedByUserId,
    DateTime SpecialDiscountApprovedAtUtc);