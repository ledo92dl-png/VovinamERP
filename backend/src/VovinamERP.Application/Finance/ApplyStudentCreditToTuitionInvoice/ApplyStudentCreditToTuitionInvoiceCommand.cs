using MediatR;

namespace VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;

public sealed record ApplyStudentCreditToTuitionInvoiceCommand(
    Guid TenantId,
    Guid StudentId,
    Guid TuitionInvoiceId,
    decimal Amount,
    DateOnly TransactionDate,
    string? Note)
    : IRequest<ApplyStudentCreditToTuitionInvoiceResult>;