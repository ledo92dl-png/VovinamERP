using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class StudentCreditTransaction : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }

    public StudentCreditTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly TransactionDate { get; private set; }

    public Guid? ReceiptId { get; private set; }
    public Guid? ReceiptItemId { get; private set; }
    public Guid? TuitionInvoiceId { get; private set; }
    public Guid? TuitionAdjustmentId { get; private set; }

    public string? Description { get; private set; }

    private StudentCreditTransaction()
    {
    }

    private StudentCreditTransaction(
        Guid tenantId,
        Guid studentId,
        StudentCreditTransactionType transactionType,
        decimal amount,
        DateOnly transactionDate,
        Guid? receiptId,
        Guid? receiptItemId,
        Guid? tuitionInvoiceId,
        string? description)
    {
        TenantId = tenantId;
        StudentId = studentId;
        TransactionType = transactionType;
        Amount = amount;
        TransactionDate = transactionDate;
        ReceiptId = receiptId;
        ReceiptItemId = receiptItemId;
        TuitionInvoiceId = tuitionInvoiceId;
        Description = description?.Trim();
    }
    
public Result LinkToTuitionAdjustment(
    Guid tuitionAdjustmentId)
{
    if (tuitionAdjustmentId == Guid.Empty)
    {
        return Result.Failure(
            new Error(
                "FIN_077",
                "Tuition adjustment identifier is required."));
    }

    if (TuitionAdjustmentId.HasValue)
    {
        return Result.Failure(
            new Error(
                "FIN_078",
                "Student credit transaction is already linked to an adjustment."));
    }

    if (TransactionType != StudentCreditTransactionType.Credit ||
        !TuitionInvoiceId.HasValue)
    {
        return Result.Failure(
            new Error(
                "FIN_079",
                "Only invoice-related credit transactions can be linked to an adjustment."));
    }

    TuitionAdjustmentId = tuitionAdjustmentId;

    return Result.Success();
}
    public static Result<StudentCreditTransaction> Create(
        Guid tenantId,
        Guid studentId,
        StudentCreditTransactionType transactionType,
        decimal amount,
        DateOnly transactionDate,
        Guid? receiptId,
        Guid? receiptItemId,
        Guid? tuitionInvoiceId,
        string? description)
    {
        if (tenantId == Guid.Empty)
        {
            return Result<StudentCreditTransaction>.Failure(
                FinanceErrors.TenantRequired);
        }

        if (studentId == Guid.Empty)
        {
            return Result<StudentCreditTransaction>.Failure(
                FinanceErrors.StudentRequired);
        }

        if (!Enum.IsDefined(transactionType))
        {
            return Result<StudentCreditTransaction>.Failure(
                new Error(
                    "FIN_049",
                    "Student credit transaction type is invalid."));
        }

        if (amount <= 0)
        {
            return Result<StudentCreditTransaction>.Failure(
                FinanceErrors.AmountMustBePositive);
        }

        return Result<StudentCreditTransaction>.Success(
            new StudentCreditTransaction(
                tenantId,
                studentId,
                transactionType,
                amount,
                transactionDate,
                receiptId,
                receiptItemId,
                tuitionInvoiceId,
                description));
    }
}