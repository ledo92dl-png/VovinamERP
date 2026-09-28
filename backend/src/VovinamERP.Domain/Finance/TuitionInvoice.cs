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
    public decimal SpecialDiscountAmount { get; private set; }

public TuitionSpecialDiscountType? SpecialDiscountType { get; private set; }

public decimal? SpecialDiscountValue { get; private set; }

public string? SpecialDiscountReason { get; private set; }

public Guid? SpecialDiscountApprovedByUserId { get; private set; }

public DateTime? SpecialDiscountApprovedAtUtc { get; private set; }
    
public decimal PaidAmount { get; private set; }

// Tổng tiền đã điều chuyển sang Student Credit hoặc hoàn tiền.
public decimal ReallocatedAmount { get; private set; }

// Số tiền thực tế còn được phân bổ cho hóa đơn.
public decimal EffectivePaidAmount =>
    PaidAmount - ReallocatedAmount;

public decimal BalanceAmount =>
    Amount
    - DiscountAmount
    - SpecialDiscountAmount
    - EffectivePaidAmount;
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
SpecialDiscountAmount = 0;
SpecialDiscountType = null;
SpecialDiscountValue = null;
SpecialDiscountReason = null;
SpecialDiscountApprovedByUserId = null;
SpecialDiscountApprovedAtUtc = null;
PaidAmount = 0;
ReallocatedAmount = 0;
        Note = note?.Trim();
        
        Status = BalanceAmount == 0
    ? TuitionInvoiceStatus.Paid
    : TuitionInvoiceStatus.Unpaid;

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
    Guid receiptItemId,
    string paymentNumber,
    decimal amount,
    TuitionPaymentMethod method,
    DateOnly paymentDate,
    string? note)
{
    if (IsArchived)
        return Result<TuitionPayment>.Failure(
            FinanceErrors.AlreadyArchived);

    if (Status == TuitionInvoiceStatus.Paid)
        return Result<TuitionPayment>.Failure(
            FinanceErrors.InvoiceAlreadyPaid);

    if (receiptItemId == Guid.Empty)
        return Result<TuitionPayment>.Failure(
            new Error(
                "FIN_042",
                "Receipt item is required."));

    if (amount <= 0)
        return Result<TuitionPayment>.Failure(
            FinanceErrors.AmountMustBePositive);

    if (amount > BalanceAmount)
        return Result<TuitionPayment>.Failure(
            FinanceErrors.PaymentExceedsBalance);

    var payment = TuitionPayment.Create(
        TenantId,
        Id,
        receiptItemId,
        paymentNumber,
        amount,
        method,
        paymentDate,
        note);

    if (payment.IsFailure || payment.Value is null)
        return Result<TuitionPayment>.Failure(
            payment.Error);

    _payments.Add(payment.Value);

    PaidAmount += amount;

    Status = BalanceAmount == 0
        ? TuitionInvoiceStatus.Paid
        : TuitionInvoiceStatus.PartiallyPaid;

    RaiseDomainEvent(
        new TuitionPaymentRecordedEvent(
            Id,
            payment.Value.Id,
            amount));

    if (Status == TuitionInvoiceStatus.Paid)
    {
        RaiseDomainEvent(
            new TuitionInvoicePaidEvent(Id));
    }

    return Result<TuitionPayment>.Success(
        payment.Value);
}

    public Result ApplyCredit(
    decimal amount)
{
    if (IsArchived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (Status == TuitionInvoiceStatus.Paid)
    {
        return Result.Failure(
            FinanceErrors.InvoiceAlreadyPaid);
    }

    if (amount <= 0)
    {
        return Result.Failure(
            FinanceErrors.AmountMustBePositive);
    }

    if (amount > BalanceAmount)
    {
        return Result.Failure(
            FinanceErrors.PaymentExceedsBalance);
    }

    PaidAmount += amount;

    Status = BalanceAmount == 0
        ? TuitionInvoiceStatus.Paid
        : TuitionInvoiceStatus.PartiallyPaid;

    if (Status == TuitionInvoiceStatus.Paid)
    {
        RaiseDomainEvent(
            new TuitionInvoicePaidEvent(Id));
    }

    return Result.Success();
}

    public Result ApplySpecialDiscount(
    TuitionSpecialDiscountType discountType,
    decimal? discountValue,
    string reason,
    Guid approvedByUserId)
{
    if (IsArchived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (string.IsNullOrWhiteSpace(reason))
    {
        return Result.Failure(
            new Error(
                "FIN_057",
                "Special discount reason is required."));
    }

    if (approvedByUserId == Guid.Empty)
    {
        return Result.Failure(
            new Error(
                "FIN_058",
                "Special discount approver is required."));
    }

    var amountBeforeSpecialDiscount =
        Amount - DiscountAmount;

    decimal specialDiscountAmount;
    decimal storedDiscountValue;

    switch (discountType)
    {
        case TuitionSpecialDiscountType.FixedAmount:
            if (!discountValue.HasValue ||
                discountValue.Value <= 0)
            {
                return Result.Failure(
                    FinanceErrors.AmountMustBePositive);
            }

            specialDiscountAmount =
                discountValue.Value;

            storedDiscountValue =
                discountValue.Value;

            break;

        case TuitionSpecialDiscountType.Percentage:
            if (!discountValue.HasValue ||
                discountValue.Value <= 0 ||
                discountValue.Value > 100)
            {
                return Result.Failure(
                    new Error(
                        "FIN_060",
                        "Special discount percentage must be greater than 0 and not exceed 100."));
            }

            storedDiscountValue =
                discountValue.Value;

            specialDiscountAmount =
                Math.Round(
                    amountBeforeSpecialDiscount *
                    discountValue.Value / 100m,
                    0,
                    MidpointRounding.AwayFromZero);

            break;

        case TuitionSpecialDiscountType.FullExemption:
            storedDiscountValue = 100m;

            specialDiscountAmount =
                amountBeforeSpecialDiscount;

            break;

        default:
            return Result.Failure(
                new Error(
                    "FIN_061",
                    "Special discount type is invalid."));
    }

    var maximumDiscount =
        amountBeforeSpecialDiscount -
        PaidAmount;

    if (specialDiscountAmount > maximumDiscount)
    {
        return Result.Failure(
            new Error(
                "FIN_059",
                "Special discount exceeds the remaining amount that can be discounted."));
    }

    SpecialDiscountType = discountType;
SpecialDiscountValue = storedDiscountValue;
SpecialDiscountAmount = specialDiscountAmount;
SpecialDiscountReason = reason.Trim();
SpecialDiscountApprovedByUserId = approvedByUserId;

SpecialDiscountApprovedAtUtc = DateTime.UtcNow;

MarkUpdated(approvedByUserId);

// Khi chưa phát sinh tiền dư, có thể cập nhật
// trạng thái ngay sau khi áp dụng giảm học phí.
// Nếu có tiền dư, ReallocateOverpayment()
// sẽ tính lại trạng thái sau khi phân bổ.
var newPayableAmount =
    Amount - DiscountAmount - SpecialDiscountAmount;

if (EffectivePaidAmount <= newPayableAmount)
{
    Status = BalanceAmount == 0
        ? TuitionInvoiceStatus.Paid
        : EffectivePaidAmount > 0
            ? TuitionInvoiceStatus.PartiallyPaid
            : TuitionInvoiceStatus.Unpaid;
}

return Result.Success();
}

public Result ApplyPostPaymentDiscount(
    decimal newSpecialDiscountAmount,
    string reason,
    Guid approvedByUserId)
{
    if (IsArchived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (approvedByUserId == Guid.Empty ||
        string.IsNullOrWhiteSpace(reason) ||
        reason.Trim().Length > 500)
    {
        return Result.Failure(
            new Error(
                "FIN_081",
                "A valid approver and adjustment reason are required."));
    }

    var maximumSpecialDiscount =
        Amount - DiscountAmount;

    if (newSpecialDiscountAmount <= SpecialDiscountAmount ||
        newSpecialDiscountAmount > maximumSpecialDiscount)
    {
        return Result.Failure(
            new Error(
                "FIN_082",
                "The new special discount must increase the existing discount without exceeding the payable amount."));
    }

    SpecialDiscountAmount = newSpecialDiscountAmount;
    SpecialDiscountType =
        TuitionSpecialDiscountType.FixedAmount;
    SpecialDiscountValue = newSpecialDiscountAmount;
    SpecialDiscountReason = reason.Trim();
    SpecialDiscountApprovedByUserId = approvedByUserId;
    SpecialDiscountApprovedAtUtc = DateTime.UtcNow;

    MarkUpdated(approvedByUserId);

    // Chưa cập nhật trạng thái ở đây.
    // Nếu phát sinh tiền dư, phải điều chuyển tiền trước.
    return Result.Success();
}
    
public decimal GetAdditionalDiscountAmount(
    decimal newSpecialDiscountAmount)
{
    return newSpecialDiscountAmount - SpecialDiscountAmount;
}

public decimal GetRequiredSettlementAmount(
    decimal newSpecialDiscountAmount)
{
    var newPayableAmount =
        Amount - DiscountAmount - newSpecialDiscountAmount;

    var overpayment =
        EffectivePaidAmount - newPayableAmount;

    return Math.Max(0m, overpayment);
}
public Result ReallocateOverpayment(
    decimal amount,
    Guid approvedByUserId)
{
    if (IsArchived)
    {
        return Result.Failure(
            FinanceErrors.AlreadyArchived);
    }

    if (approvedByUserId == Guid.Empty)
    {
        return Result.Failure(
            new Error(
                "FIN_075",
                "Adjustment approver is required."));
    }

    if (amount <= 0)
    {
        return Result.Failure(
            FinanceErrors.AmountMustBePositive);
    }

    var payableAmount =
        Amount - DiscountAmount - SpecialDiscountAmount;

    var availableOverpayment =
        EffectivePaidAmount - payableAmount;

    if (amount > availableOverpayment)
    {
        return Result.Failure(
            new Error(
                "FIN_076",
                "Reallocation exceeds available overpayment."));
    }

    ReallocatedAmount += amount;

    MarkUpdated(approvedByUserId);

    Status = BalanceAmount == 0
        ? TuitionInvoiceStatus.Paid
        : EffectivePaidAmount > 0
            ? TuitionInvoiceStatus.PartiallyPaid
            : TuitionInvoiceStatus.Unpaid;

    return Result.Success();
}

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = TuitionInvoiceStatus.Archived;
        base.Archive(userId);
    }
}
