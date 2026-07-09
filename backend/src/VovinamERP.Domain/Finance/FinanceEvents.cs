using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Finance;

public sealed record TuitionInvoiceCreatedEvent(
    Guid TuitionInvoiceId,
    Guid StudentId,
    int Year,
    int Month,
    decimal Amount
) : DomainEvent;

public sealed record TuitionPaymentRecordedEvent(
    Guid TuitionInvoiceId,
    Guid TuitionPaymentId,
    decimal Amount
) : DomainEvent;

public sealed record TuitionInvoicePaidEvent(
    Guid TuitionInvoiceId
) : DomainEvent;

public sealed record ReceiptConfirmedEvent(
    Guid ReceiptId,
    string ReceiptNumber
) : DomainEvent;
