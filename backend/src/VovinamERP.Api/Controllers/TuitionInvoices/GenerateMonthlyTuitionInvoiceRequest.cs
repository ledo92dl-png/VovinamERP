namespace VovinamERP.Api.Controllers.TuitionInvoices;

public sealed record GenerateMonthlyTuitionInvoiceRequest(
    Guid TenantId,
    Guid StudentId,
    string InvoiceNumber,
    int Year,
    int Month,
    decimal StandardAmount,
    string? Note);