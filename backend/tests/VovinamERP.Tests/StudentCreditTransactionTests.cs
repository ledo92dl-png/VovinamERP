
using VovinamERP.Domain.Finance;

namespace VovinamERP.Tests;

public class StudentCreditTransactionTests
{
    [Fact]
    public void TuitionAdjustment_ShouldCreateLinkedStudentCredit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var adjustmentId = Guid.NewGuid();

        // Act: Tạo khoản tiền dư 50.000 đồng.
        var createResult = StudentCreditTransaction.Create(
            tenantId,
            studentId,
            StudentCreditTransactionType.Credit,
            50_000m,
            new DateOnly(2027, 1, 5),
            null,
            null,
            invoiceId,
            "Tuition adjustment");

        Assert.True(createResult.IsSuccess);

        var credit = Assert.IsType<StudentCreditTransaction>(
            createResult.Value);

        // Liên kết với lịch sử điều chỉnh học phí.
        var linkResult =
            credit.LinkToTuitionAdjustment(adjustmentId);

        // Assert
        Assert.True(linkResult.IsSuccess);
        Assert.Equal(tenantId, credit.TenantId);
        Assert.Equal(studentId, credit.StudentId);
        Assert.Equal(invoiceId, credit.TuitionInvoiceId);
        Assert.Equal(adjustmentId, credit.TuitionAdjustmentId);
        Assert.Equal(50_000m, credit.Amount);

        Assert.Equal(
            StudentCreditTransactionType.Credit,
            credit.TransactionType);

        // Khoản tiền này không phải khoản thu mới.
        Assert.Null(credit.ReceiptId);
        Assert.Null(credit.ReceiptItemId);
    }

    [Fact]
    public void StudentCredit_ShouldNotLinkToTwoAdjustments()
    {
        var createResult = StudentCreditTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StudentCreditTransactionType.Credit,
            50_000m,
            new DateOnly(2027, 1, 5),
            null,
            null,
            Guid.NewGuid(),
            null);

        Assert.True(createResult.IsSuccess);

        var credit = Assert.IsType<StudentCreditTransaction>(
            createResult.Value);

        var firstAdjustmentId = Guid.NewGuid();

        var firstLink =
            credit.LinkToTuitionAdjustment(firstAdjustmentId);

        var secondLink =
            credit.LinkToTuitionAdjustment(Guid.NewGuid());

        Assert.True(firstLink.IsSuccess);
        Assert.True(secondLink.IsFailure);

        // Không được thay đổi liên kết ban đầu.
        Assert.Equal(
            firstAdjustmentId,
            credit.TuitionAdjustmentId);
    }

    [Fact]
    public void DebitTransaction_ShouldNotLinkToTuitionAdjustment()
    {
        var createResult = StudentCreditTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StudentCreditTransactionType.Debit,
            50_000m,
            new DateOnly(2027, 1, 5),
            null,
            null,
            Guid.NewGuid(),
            null);

        Assert.True(createResult.IsSuccess);

        var debit = Assert.IsType<StudentCreditTransaction>(
            createResult.Value);

        var linkResult =
            debit.LinkToTuitionAdjustment(Guid.NewGuid());

        Assert.True(linkResult.IsFailure);
        Assert.Null(debit.TuitionAdjustmentId);
    }
}