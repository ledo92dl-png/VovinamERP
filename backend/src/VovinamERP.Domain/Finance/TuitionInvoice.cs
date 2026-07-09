using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class TuitionInvoice : AggregateRoot
{
    private readonly List<TuitionPayment> _payments = [];

    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }

    public string InvoiceNumber { get; private set; } = default!;
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal Amount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal BalanceAmount => Amount - DiscountAmount - PaidAmount;
    public string? Note { get; private set; }
    public TuitionInvoiceStatus Status { get; private set; }

    public IReadOnlyCollection<TuitionPayment> Payments => _payments.AsReadOnly();

    private TuitionInvoice() { }

    private TuitionInvoice(
        Guid tenantId,
        Guid studentId,
        string invoiceNumber,
        int year,
        int month,
        decimal amount,
        decimal discountAmount,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        InvoiceNumber = invoiceNumber.Trim();
        Year = year;
        Month = month;
        Amount = amount;
        DiscountAmount = discountAmount;
        PaidAmount = 0;
        Note = note?.Trim();
        Status = TuitionInvoiceStatus.Unpaid;

        RaiseDomainEvent(new TuitionInvoiceCreatedEvent(Id, StudentId, Year, Month, Amount));
    }

    public static Result<TuitionInvoice> CreateMonthlyInvoice(
        Guid tenantId,
        Guid studentId,
        string invoiceNumber,
        int year,
        int month,
        decimal amount,
        decimal discountAmount,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<TuitionInvoice>.Failure(FinanceErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<TuitionInvoice>.Failure(FinanceErrors.StudentRequired);

        if (month < 1 || month > 12 || year < 2000)
            return Result<TuitionInvoice>.Failure(FinanceErrors.BillingMonthInvalid);

        if (amount <= 0 || discountAmount < 0 || discountAmount > amount)
            return Result<TuitionInvoice>.Failure(FinanceErrors.AmountMustBePositive);

        return Result<TuitionInvoice>.Success(
            new TuitionInvoice(tenantId, studentId, invoiceNumber, year, month, amount, discountAmount, note));
    }

    public Result<TuitionPayment> RecordPayment(
        string paymentNumber,
        decimal amount,
        TuitionPaymentMethod method,
        DateOnly paymentDate,
        string? note)
    {
        if (IsArchived)
            return Result<TuitionPayment>.Failure(FinanceErrors.AlreadyArchived);

        if (Status == TuitionInvoiceStatus.Paid)
            return Result<TuitionPayment>.Failure(FinanceErrors.InvoiceAlreadyPaid);

        if (amount <= 0)
            return Result<TuitionPayment>.Failure(FinanceErrors.AmountMustBePositive);

        if (amount > BalanceAmount)
            return Result<TuitionPayment>.Failure(FinanceErrors.PaymentExceedsBalance);

        var payment = TuitionPayment.Create(
            TenantId,
            Id,
            paymentNumber,
            amount,
            method,
            paymentDate,
            note);

        if (payment.IsFailure || payment.Value is null)
            return Result<TuitionPayment>.Failure(payment.Error);

        _payments.Add(payment.Value);
        PaidAmount += amount;
        Status = BalanceAmount == 0
            ? TuitionInvoiceStatus.Paid
            : TuitionInvoiceStatus.PartiallyPaid;

        RaiseDomainEvent(new TuitionPaymentRecordedEvent(Id, payment.Value.Id, amount));

        if (Status == TuitionInvoiceStatus.Paid)
            RaiseDomainEvent(new TuitionInvoicePaidEvent(Id));

        return Result<TuitionPayment>.Success(payment.Value);
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = TuitionInvoiceStatus.Archived;
        base.Archive(userId);
    }
}
