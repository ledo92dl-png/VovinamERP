
using VovinamERP.Domain.Finance;

namespace VovinamERP.Tests;

public class TuitionInvoiceAdjustmentTests
{
    [Fact]
    public void PaidInvoice_AfterAdditionalDiscount_ShouldReallocateOverpayment()
    {
        // Arrange: Tạo hóa đơn học phí 300.000 đồng.
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var createResult = TuitionInvoice.CreateMonthlyInvoice(
            tenantId,
            studentId,
            "HP-TEST-001",
            2027,
            1,
            300_000m,
            0m,
            null);

        Assert.True(createResult.IsSuccess);

        var invoice = Assert.IsType<TuitionInvoice>(
            createResult.Value);

        // Ghi nhận thanh toán đủ 300.000 đồng.
        var paymentResult = invoice.RecordPayment(
            Guid.NewGuid(),
            "PAY-TEST-001",
            300_000m,
            TuitionPaymentMethod.Cash,
            new DateOnly(2027, 1, 5),
            null);

        Assert.True(paymentResult.IsSuccess);
        Assert.Equal(300_000m, invoice.PaidAmount);
        Assert.Equal(TuitionInvoiceStatus.Paid, invoice.Status);

        // Act: Giảm thêm 50.000 đồng.
        var settlementAmount =
            invoice.GetRequiredSettlementAmount(50_000m);

        var discountResult =
            invoice.ApplyPostPaymentDiscount(
                50_000m,
                "Giam hoc phi sau thanh toan",
                approverId);

        Assert.True(discountResult.IsSuccess);

        var reallocationResult =
            invoice.ReallocateOverpayment(
                settlementAmount,
                approverId);

        // Assert: Kiểm tra kết quả.
        Assert.True(reallocationResult.IsSuccess);

        Assert.Equal(50_000m, settlementAmount);
        Assert.Equal(50_000m, invoice.SpecialDiscountAmount);

        // Giữ nguyên số tiền đã thanh toán ban đầu.
        Assert.Equal(300_000m, invoice.PaidAmount);

        // Chuyển 50.000 đồng ra khỏi hóa đơn.
        Assert.Equal(50_000m, invoice.ReallocatedAmount);

        // Số tiền thực tế phân bổ cho hóa đơn còn 250.000 đồng.
        Assert.Equal(250_000m, invoice.EffectivePaidAmount);

        Assert.Equal(0m, invoice.BalanceAmount);
        Assert.Equal(TuitionInvoiceStatus.Paid, invoice.Status);

        // Chứng từ thanh toán ban đầu vẫn được giữ nguyên.
        Assert.Single(invoice.Payments);
        Assert.Equal(
            300_000m,
            invoice.Payments.Single().Amount);
    }
}