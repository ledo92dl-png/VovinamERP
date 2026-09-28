namespace VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;

public sealed record ApplyStudentCreditToTuitionInvoiceResult(
    Guid TuitionInvoiceId,
    Guid StudentId,
    decimal AppliedCreditAmount,
    decimal RemainingCreditBalance,
    decimal InvoicePaidAmount,
    decimal InvoiceBalanceAmount,
    string InvoiceStatus);