using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.GetStudentTuitionHistory;

public sealed record StudentTuitionHistoryItem(
    Guid TuitionInvoiceId,
    string InvoiceNumber,
    int Year,
    int Month,
    int AttendanceCount,
    decimal StandardAmount,
    decimal AttendanceDiscountAmount,
    TuitionSpecialDiscountType? SpecialDiscountType,
    decimal? SpecialDiscountValue,
    decimal SpecialDiscountAmount,
    decimal FinalPayableAmount,
    decimal CreditAppliedAmount,
    decimal ReceiptPaidAmount,
    decimal TotalSettledAmount,
    decimal BalanceAmount,
    TuitionInvoiceStatus Status,
    string? SpecialDiscountReason,
    Guid? SpecialDiscountApprovedByUserId,
    DateTime? SpecialDiscountApprovedAtUtc,
    string? Note);