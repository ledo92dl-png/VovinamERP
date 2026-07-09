namespace VovinamERP.Domain.Finance;

public enum TuitionInvoiceStatus
{
    Draft = 1,
    Unpaid = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Cancelled = 5,
    Archived = 6
}
