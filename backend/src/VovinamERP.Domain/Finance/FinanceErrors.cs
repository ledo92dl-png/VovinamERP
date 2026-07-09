using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public static class FinanceErrors
{
    public static readonly Error TenantRequired =
        new("FIN_001", "Tenant is required.");

    public static readonly Error StudentRequired =
        new("FIN_002", "Student is required.");

    public static readonly Error AmountMustBePositive =
        new("FIN_003", "Amount must be greater than zero.");

    public static readonly Error BillingMonthInvalid =
        new("FIN_004", "Billing month is invalid.");

    public static readonly Error InvoiceAlreadyPaid =
        new("FIN_005", "Invoice has already been paid.");

    public static readonly Error PaymentExceedsBalance =
        new("FIN_006", "Payment amount exceeds invoice balance.");

    public static readonly Error ConfirmedReceiptCannotBeEdited =
        new("FIN_007", "Confirmed receipt cannot be edited.");

    public static readonly Error AlreadyArchived =
        new("FIN_008", "Record has already been archived.");
}
