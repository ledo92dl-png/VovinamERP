using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class TuitionPayment : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid TuitionInvoiceId { get; private set; }
    public Guid ReceiptItemId { get; private set; }
    public string PaymentNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public TuitionPaymentMethod PaymentMethod { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public string? Note { get; private set; }

    private TuitionPayment() { }

    private TuitionPayment(
    Guid tenantId,
    Guid tuitionInvoiceId,
    Guid receiptItemId,
    string paymentNumber,
    decimal amount,
    TuitionPaymentMethod paymentMethod,
    DateOnly paymentDate,
    string? note)
{
    TenantId = tenantId;
    TuitionInvoiceId = tuitionInvoiceId;
    ReceiptItemId = receiptItemId;
    PaymentNumber = paymentNumber.Trim();
    Amount = amount;
    PaymentMethod = paymentMethod;
    PaymentDate = paymentDate;
    Note = note?.Trim();
}

    public static Result<TuitionPayment> Create(
    Guid tenantId,
    Guid tuitionInvoiceId,
    Guid receiptItemId,
    string paymentNumber,
    decimal amount,
    TuitionPaymentMethod paymentMethod,
    DateOnly paymentDate,
    string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<TuitionPayment>.Failure(FinanceErrors.TenantRequired);

        if (tuitionInvoiceId == Guid.Empty)
            return Result<TuitionPayment>.Failure(new Error("FIN_009", "Tuition invoice is required."));

        if (amount <= 0)
            return Result<TuitionPayment>.Failure(FinanceErrors.AmountMustBePositive);

        if (receiptItemId == Guid.Empty)
    return Result<TuitionPayment>.Failure(
        new Error("FIN_042", "Receipt item is required."));

        return Result<TuitionPayment>.Success(
    new TuitionPayment(
        tenantId,
        tuitionInvoiceId,
        receiptItemId,
        paymentNumber,
        amount,
        paymentMethod,
        paymentDate,
        note));
    }
}
