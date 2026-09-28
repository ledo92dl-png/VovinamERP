namespace VovinamERP.Api.Controllers.TuitionInvoices;

public sealed record ApplyStudentCreditRequest(
    Guid TenantId,
    Guid StudentId,
    decimal Amount,
    DateOnly TransactionDate,
    string? Note);