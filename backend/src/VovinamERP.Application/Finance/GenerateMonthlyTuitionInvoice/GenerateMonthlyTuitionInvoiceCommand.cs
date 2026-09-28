using MediatR;

namespace VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;

public sealed record GenerateMonthlyTuitionInvoiceCommand(
    Guid TenantId,
    Guid StudentId,
    string InvoiceNumber,
    int Year,
    int Month,
    decimal StandardAmount,
    string? Note)
    : IRequest<GenerateMonthlyTuitionInvoiceResult>;