using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;

public sealed record GenerateMonthlyTuitionInvoiceResult(
    Guid TuitionInvoiceId,
    Guid TenantId,
    Guid StudentId,
    string InvoiceNumber,
    int Year,
    int Month,
    int AttendanceCount,
    decimal StandardAmount,
    decimal ChargeRate,
    decimal DiscountAmount,
    decimal PayableAmount,
    decimal PaidAmount,
    decimal BalanceAmount,
    TuitionInvoiceStatus Status,
    string? Note);