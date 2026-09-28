using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class ReceiptItem : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid ReceiptId { get; private set; }

    // Môn sinh mà khoản thu này thuộc về.
    // Có thể null đối với tài trợ hoặc khoản thu chung.
    public Guid? StudentId { get; private set; }

    public ReceiptItemType ItemType { get; private set; }

    /// <summary>
    /// Mã tham chiếu đến nghiệp vụ gốc, ví dụ:
    /// TuitionInvoiceId, ProductId hoặc ExamRegistrationId.
    /// Có thể để null đối với khoản tài trợ hoặc khoản thu khác.
    /// </summary>
    public Guid? ReferenceId { get; private set; }

    public string Description { get; private set; } = default!;

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public decimal GrossAmount => Quantity * UnitPrice;
    public decimal TotalAmount => GrossAmount - DiscountAmount;

    public string? Note { get; private set; }

    private ReceiptItem()
    {
    }

    private ReceiptItem(
        Guid tenantId,
        Guid receiptId,
        Guid? studentId,
        ReceiptItemType itemType,
        Guid? referenceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        string? note)
    {
        TenantId = tenantId;
        ReceiptId = receiptId;
        StudentId = studentId;
        ItemType = itemType;
        ReferenceId = referenceId;
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        Note = note?.Trim();
    }

    public static Result<ReceiptItem> Create(
        Guid tenantId,
        Guid receiptId,
        Guid? studentId,
        ReceiptItemType itemType,
        Guid? referenceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        string? note)
    {
        if (tenantId == Guid.Empty)
        {
            return Result<ReceiptItem>.Failure(
                FinanceErrors.TenantRequired);
        }

        if (receiptId == Guid.Empty)
        {
            return Result<ReceiptItem>.Failure(
                new Error(
                    "FIN_010",
                    "Receipt is required."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<ReceiptItem>.Failure(
                new Error(
                    "FIN_011",
                    "Receipt item description is required."));
        }

        if (quantity <= 0)
        {
            return Result<ReceiptItem>.Failure(
                new Error(
                    "FIN_012",
                    "Receipt item quantity must be greater than zero."));
        }

        if (unitPrice < 0)
        {
            return Result<ReceiptItem>.Failure(
                new Error(
                    "FIN_013",
                    "Receipt item unit price cannot be negative."));
        }

        var grossAmount = quantity * unitPrice;

        if (discountAmount < 0 ||
            discountAmount > grossAmount)
        {
            return Result<ReceiptItem>.Failure(
                new Error(
                    "FIN_014",
                    "Receipt item discount is invalid."));
        }

        return Result<ReceiptItem>.Success(
            new ReceiptItem(
                tenantId,
                receiptId,
                studentId,
                itemType,
                referenceId,
                description,
                quantity,
                unitPrice,
                discountAmount,
                note));
    }

    public Result Update(
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        string? note)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(
                new Error(
                    "FIN_011",
                    "Receipt item description is required."));
        }

        if (quantity <= 0)
        {
            return Result.Failure(
                new Error(
                    "FIN_012",
                    "Receipt item quantity must be greater than zero."));
        }

        if (unitPrice < 0)
        {
            return Result.Failure(
                new Error(
                    "FIN_013",
                    "Receipt item unit price cannot be negative."));
        }

        var grossAmount = quantity * unitPrice;

        if (discountAmount < 0 ||
            discountAmount > grossAmount)
        {
            return Result.Failure(
                new Error(
                    "FIN_014",
                    "Receipt item discount is invalid."));
        }

        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        Note = note?.Trim();

        return Result.Success();
    }
    public Result AssignStudent(Guid studentId)
{
    if (studentId == Guid.Empty)
    {
        return Result.Failure(
            new Error(
                "FIN_025",
                "Student is required."));
    }

    if (StudentId == studentId)
    {
        return Result.Success();
    }

    StudentId = studentId;

    return Result.Success();
}
}