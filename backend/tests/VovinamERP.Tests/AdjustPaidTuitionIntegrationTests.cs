using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.AdjustPaidTuition;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;
using VovinamERP.Application.Finance.ConfirmTuitionRefund;
using VovinamERP.Application.Finance.ReconcileTuitionRefund;

namespace VovinamERP.Tests;

public class AdjustPaidTuitionIntegrationTests
{
    [Fact]
    public async Task PaidInvoice_AdjustToStudentCredit_ShouldPersistEverything()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var invoiceResult =
            TuitionInvoice.CreateMonthlyInvoice(
                tenantId,
                studentId,
                $"HP-INT-{Guid.NewGuid():N}",
                2027,
                2,
                300_000m,
                0m,
                null);

        Assert.True(invoiceResult.IsSuccess);
        Assert.NotNull(invoiceResult.Value);

        var invoice = invoiceResult.Value!;

        var paymentResult =
            invoice.RecordPayment(
                Guid.NewGuid(),
                $"PAY-INT-{Guid.NewGuid():N}",
                300_000m,
                TuitionPaymentMethod.Cash,
                new DateOnly(2027, 2, 5),
                null);

        Assert.True(paymentResult.IsSuccess);

        db.TuitionInvoices.Add(invoice);

        await db.SaveChangesAsync();

        var handler =
            new AdjustPaidTuitionCommandHandler(
                new TuitionInvoiceRepository(db),
                new TuitionAdjustmentRepository(db),
                new TuitionRefundRepository(db),
                new StudentCreditRepository(db),
                new TuitionAdjustmentTransaction(db),
                db);

        var command =
            new AdjustPaidTuitionCommand(
                tenantId,
                invoice.Id,
                50_000m,
                TuitionAdjustmentSettlementType.StudentCredit,
                "Integration test - post payment discount",
                approverId);

        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        Assert.True(
            result.IsSuccess,
            result.IsFailure
                ? $"{result.Error.Code}: {result.Error.Message}"
                : null);

        Assert.NotNull(result.Value);

        var value = result.Value!;

        Assert.Equal(300_000m, value.PreviousPayableAmount);
        Assert.Equal(250_000m, value.NewPayableAmount);
        Assert.Equal(50_000m, value.AdjustmentAmount);
        Assert.Equal(50_000m, value.SettlementAmount);
        Assert.Equal(0m, value.RemainingBalance);

        Assert.Equal(
            TuitionAdjustmentSettlementType.StudentCredit,
            value.SettlementType);

        Assert.NotNull(value.StudentCreditTransactionId);
        Assert.Null(value.TuitionRefundId);
        Assert.Null(value.RefundStatus);

        db.ChangeTracker.Clear();

        var persistedInvoice =
            await db.TuitionInvoices
                .SingleAsync(x => x.Id == invoice.Id);

        Assert.Equal(
            50_000m,
            persistedInvoice.SpecialDiscountAmount);

        Assert.Equal(
            300_000m,
            persistedInvoice.PaidAmount);

        Assert.Equal(
            50_000m,
            persistedInvoice.ReallocatedAmount);

        Assert.Equal(
            250_000m,
            persistedInvoice.EffectivePaidAmount);

        Assert.Equal(
            0m,
            persistedInvoice.BalanceAmount);

        Assert.Equal(
            TuitionInvoiceStatus.Paid,
            persistedInvoice.Status);

        var adjustment =
            await db.TuitionAdjustments
                .SingleAsync(
                    x => x.Id == value.TuitionAdjustmentId);

        Assert.Equal(invoice.Id, adjustment.TuitionInvoiceId);
        Assert.Equal(studentId, adjustment.StudentId);
        Assert.Equal(50_000m, adjustment.AdjustmentAmount);
        Assert.Equal(50_000m, adjustment.SettlementAmount);

        Assert.Equal(
            TuitionAdjustmentSettlementType.StudentCredit,
            adjustment.SettlementType);

        var credit =
            await db.StudentCreditTransactions
                .SingleAsync(
                    x => x.Id ==
                         value.StudentCreditTransactionId!.Value);

        Assert.Equal(studentId, credit.StudentId);
        Assert.Equal(50_000m, credit.Amount);

        Assert.Equal(
            StudentCreditTransactionType.Credit,
            credit.TransactionType);

        Assert.Equal(
            invoice.Id,
            credit.TuitionInvoiceId);

        Assert.Equal(
            adjustment.Id,
            credit.TuitionAdjustmentId);
    }
    [Fact]
public async Task PaidInvoice_AdjustToRefund_ShouldCreatePendingRefund()
{
    await using var db = TestDatabase.CreateContext();

    var tenantId = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    var approverId = Guid.NewGuid();

    var invoiceResult =
        TuitionInvoice.CreateMonthlyInvoice(
            tenantId,
            studentId,
            $"HP-REF-{Guid.NewGuid():N}",
            2027,
            3,
            300_000m,
            0m,
            null);

    Assert.True(invoiceResult.IsSuccess);
    Assert.NotNull(invoiceResult.Value);

    var invoice = invoiceResult.Value!;

    var paymentResult =
        invoice.RecordPayment(
            Guid.NewGuid(),
            $"PAY-REF-{Guid.NewGuid():N}",
            300_000m,
            TuitionPaymentMethod.Cash,
            new DateOnly(2027, 3, 5),
            null);

    Assert.True(paymentResult.IsSuccess);

    db.TuitionInvoices.Add(invoice);
    await db.SaveChangesAsync();

    var handler =
        new AdjustPaidTuitionCommandHandler(
            new TuitionInvoiceRepository(db),
            new TuitionAdjustmentRepository(db),
            new TuitionRefundRepository(db),
            new StudentCreditRepository(db),
            new TuitionAdjustmentTransaction(db),
            db);

    var command =
        new AdjustPaidTuitionCommand(
            tenantId,
            invoice.Id,
            50_000m,
            TuitionAdjustmentSettlementType.Refund,
            "Integration test - refund after discount",
            approverId);

    var result =
        await handler.Handle(
            command,
            CancellationToken.None);

    Assert.True(
        result.IsSuccess,
        result.IsFailure
            ? $"{result.Error.Code}: {result.Error.Message}"
            : null);

    Assert.NotNull(result.Value);

    var value = result.Value!;

    Assert.Equal(300_000m, value.PreviousPayableAmount);
    Assert.Equal(250_000m, value.NewPayableAmount);
    Assert.Equal(50_000m, value.AdjustmentAmount);
    Assert.Equal(50_000m, value.SettlementAmount);
    Assert.Equal(0m, value.RemainingBalance);

    Assert.Equal(
        TuitionAdjustmentSettlementType.Refund,
        value.SettlementType);

    Assert.Null(value.StudentCreditTransactionId);
    Assert.NotNull(value.TuitionRefundId);

    Assert.Equal(
        TuitionRefundStatus.Pending,
        value.RefundStatus);

    db.ChangeTracker.Clear();

    var persistedInvoice =
        await db.TuitionInvoices
            .SingleAsync(x => x.Id == invoice.Id);

    Assert.Equal(
        50_000m,
        persistedInvoice.SpecialDiscountAmount);

    Assert.Equal(
        300_000m,
        persistedInvoice.PaidAmount);

    Assert.Equal(
        50_000m,
        persistedInvoice.ReallocatedAmount);

    Assert.Equal(
        250_000m,
        persistedInvoice.EffectivePaidAmount);

    Assert.Equal(0m, persistedInvoice.BalanceAmount);

    Assert.Equal(
        TuitionInvoiceStatus.Paid,
        persistedInvoice.Status);

    var adjustment =
        await db.TuitionAdjustments
            .SingleAsync(
                x => x.Id == value.TuitionAdjustmentId);

    Assert.Equal(50_000m, adjustment.AdjustmentAmount);
    Assert.Equal(50_000m, adjustment.SettlementAmount);

    Assert.Equal(
        TuitionAdjustmentSettlementType.Refund,
        adjustment.SettlementType);

    var refund =
        await db.TuitionRefunds
            .SingleAsync(
                x => x.Id == value.TuitionRefundId!.Value);

    Assert.Equal(invoice.Id, refund.TuitionInvoiceId);
    Assert.Equal(adjustment.Id, refund.TuitionAdjustmentId);
    Assert.Equal(studentId, refund.StudentId);
    Assert.Equal(50_000m, refund.Amount);

    Assert.Equal(
        TuitionRefundStatus.Pending,
        refund.Status);

    Assert.Null(refund.RefundedDate);
    Assert.Null(refund.RefundedByUserId);
    Assert.Null(refund.ReconciledAtUtc);
    Assert.Null(refund.ReconciledByUserId);
}
[Fact]
public async Task PendingRefund_ConfirmRefund_ShouldPersistRefundedState()
{
    await using var db = TestDatabase.CreateContext();

    var tenantId = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    var adjustmentId = Guid.NewGuid();
    var refundOperatorId = Guid.NewGuid();

    var refundResult =
        TuitionRefund.Create(
            tenantId,
            studentId,
            Guid.NewGuid(),
            adjustmentId,
            50_000m);

    Assert.True(refundResult.IsSuccess);
    Assert.NotNull(refundResult.Value);

    var refund = refundResult.Value!;

    db.TuitionRefunds.Add(refund);
    await db.SaveChangesAsync();

    var handler =
        new ConfirmTuitionRefundCommandHandler(
            new TuitionRefundRepository(db),
            db);

    var command =
        new ConfirmTuitionRefundCommand(
            tenantId,
            refund.Id,
            new DateOnly(2027, 3, 10),
            refundOperatorId,
            PaymentMethod.BankTransfer,
            "REF-20270310-001",
            "https://example.com/refund-evidence.jpg");

    var result =
        await handler.Handle(
            command,
            CancellationToken.None);

    Assert.True(
        result.IsSuccess,
        result.IsFailure
            ? $"{result.Error.Code}: {result.Error.Message}"
            : null);

    Assert.NotNull(result.Value);

    var value = result.Value!;

    Assert.Equal(refund.Id, value.TuitionRefundId);
    Assert.Equal(50_000m, value.Amount);

    Assert.Equal(
        TuitionRefundStatus.Refunded,
        value.Status);

    Assert.Equal(
        new DateOnly(2027, 3, 10),
        value.RefundedDate);

    Assert.Equal(
        refundOperatorId,
        value.RefundedByUserId);

    Assert.Equal(
        PaymentMethod.BankTransfer,
        value.PaymentMethod);

    Assert.Equal(
        "REF-20270310-001",
        value.TransactionReference);

    db.ChangeTracker.Clear();

    var persistedRefund =
        await db.TuitionRefunds
            .SingleAsync(x => x.Id == refund.Id);

    Assert.Equal(
        TuitionRefundStatus.Refunded,
        persistedRefund.Status);

    Assert.Equal(
        new DateOnly(2027, 3, 10),
        persistedRefund.RefundedDate);

    Assert.Equal(
        refundOperatorId,
        persistedRefund.RefundedByUserId);

    Assert.Equal(
        PaymentMethod.BankTransfer,
        persistedRefund.PaymentMethod);

    Assert.Equal(
        "REF-20270310-001",
        persistedRefund.TransactionReference);

    Assert.Equal(
        "https://example.com/refund-evidence.jpg",
        persistedRefund.EvidenceImageUrl);

    Assert.Null(persistedRefund.ReconciledByUserId);
    Assert.Null(persistedRefund.ReconciledAtUtc);
}
[Fact]
public async Task RefundedRefund_Reconcile_ShouldPersistReconciledState()
{
    await using var db = TestDatabase.CreateContext();

    var tenantId = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    var refundOperatorId = Guid.NewGuid();
    var accountantId = Guid.NewGuid();

    var refundResult =
        TuitionRefund.Create(
            tenantId,
            studentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            50_000m);

    Assert.True(refundResult.IsSuccess);
    Assert.NotNull(refundResult.Value);

    var refund = refundResult.Value!;

    var confirmResult =
        refund.ConfirmRefund(
            new DateOnly(2027, 3, 10),
            refundOperatorId,
            PaymentMethod.BankTransfer,
            "REF-RECON-001",
            "https://example.com/refund-evidence.jpg");

    Assert.True(confirmResult.IsSuccess);

    db.TuitionRefunds.Add(refund);
    await db.SaveChangesAsync();

    db.ChangeTracker.Clear();

    var handler =
        new ReconcileTuitionRefundCommandHandler(
            new TuitionRefundRepository(db),
            db);

    var command =
        new ReconcileTuitionRefundCommand(
            tenantId,
            refund.Id,
            accountantId);

    var result =
        await handler.Handle(
            command,
            CancellationToken.None);

    Assert.True(
        result.IsSuccess,
        result.IsFailure
            ? $"{result.Error.Code}: {result.Error.Message}"
            : null);

    Assert.NotNull(result.Value);

    var value = result.Value!;

    Assert.Equal(refund.Id, value.TuitionRefundId);
    Assert.Equal(50_000m, value.Amount);

    Assert.Equal(
        TuitionRefundStatus.Reconciled,
        value.Status);

    Assert.Equal(
        accountantId,
        value.ReconciledByUserId);

    Assert.NotEqual(
        default,
        value.ReconciledAtUtc);

    db.ChangeTracker.Clear();

    var persistedRefund =
        await db.TuitionRefunds
            .SingleAsync(x => x.Id == refund.Id);

    Assert.Equal(
        TuitionRefundStatus.Reconciled,
        persistedRefund.Status);

    Assert.Equal(
        accountantId,
        persistedRefund.ReconciledByUserId);

    Assert.NotNull(
        persistedRefund.ReconciledAtUtc);

    Assert.Equal(
        new DateOnly(2027, 3, 10),
        persistedRefund.RefundedDate);

    Assert.Equal(
        refundOperatorId,
        persistedRefund.RefundedByUserId);

    Assert.Equal(
        PaymentMethod.BankTransfer,
        persistedRefund.PaymentMethod);

    Assert.Equal(
        "REF-RECON-001",
        persistedRefund.TransactionReference);
}
    [Fact]
    public async Task InvoicePaidByStudentCredit_AfterDiscount_ShouldReturnOverpaymentToStudentCredit()
    {
        await using var db = TestDatabase.CreateContext();

        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        // 1. Tạo hóa đơn học phí 300.000đ.
        var invoiceResult =
            TuitionInvoice.CreateMonthlyInvoice(
                tenantId,
                studentId,
                $"HP-CREDIT-ADJ-{Guid.NewGuid():N}",
                2027,
                6,
                300_000m,
                0m,
                null);

        Assert.True(invoiceResult.IsSuccess);
        Assert.NotNull(invoiceResult.Value);

        var invoice = invoiceResult.Value!;

        // 2. Mô phỏng 300.000đ Student Credit đã được cấn vào hóa đơn.
        var applyCreditResult =
            invoice.ApplyCredit(
                300_000m);

        Assert.True(applyCreditResult.IsSuccess);

        var debitResult =
            StudentCreditTransaction.Create(
                tenantId,
                studentId,
                StudentCreditTransactionType.Debit,
                300_000m,
                new DateOnly(2027, 6, 1),
null,
null,
invoice.Id,
"Apply student credit to tuition invoice");

        Assert.True(debitResult.IsSuccess);
        Assert.NotNull(debitResult.Value);

        db.TuitionInvoices.Add(invoice);
        db.StudentCreditTransactions.Add(debitResult.Value!);

        await db.SaveChangesAsync();

        // Sau khi cấn 300.000đ, hóa đơn phải được thanh toán đủ.
        Assert.Equal(0m, invoice.BalanceAmount);
        Assert.Equal(TuitionInvoiceStatus.Paid, invoice.Status);

        var handler =
            new AdjustPaidTuitionCommandHandler(
                new TuitionInvoiceRepository(db),
                new TuitionAdjustmentRepository(db),
                new TuitionRefundRepository(db),
                new StudentCreditRepository(db),
                new TuitionAdjustmentTransaction(db),
                db);

        // 3. Sau đó Admin giảm thêm 100.000đ.
        var command =
            new AdjustPaidTuitionCommand(
                tenantId,
                invoice.Id,
                100_000m,
                TuitionAdjustmentSettlementType.StudentCredit,
                "Return overpayment from student credit",
                approverId);

        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        Assert.True(
            result.IsSuccess,
            result.IsFailure
                ? $"{result.Error.Code}: {result.Error.Message}"
                : null);

        Assert.NotNull(result.Value);

        var value = result.Value!;

        Assert.Equal(300_000m, value.PreviousPayableAmount);
        Assert.Equal(200_000m, value.NewPayableAmount);
        Assert.Equal(100_000m, value.AdjustmentAmount);
        Assert.Equal(100_000m, value.SettlementAmount);
        Assert.Equal(0m, value.RemainingBalance);

        Assert.Equal(
            TuitionAdjustmentSettlementType.StudentCredit,
            value.SettlementType);

        Assert.NotNull(value.StudentCreditTransactionId);
        Assert.Null(value.TuitionRefundId);

        // 4. Đọc lại database để kiểm tra tiền thực tế.
        db.ChangeTracker.Clear();

        var persistedInvoice =
            await db.TuitionInvoices
                .SingleAsync(x => x.Id == invoice.Id);

        Assert.Equal(
            100_000m,
            persistedInvoice.SpecialDiscountAmount);

        Assert.Equal(
            200_000m,
            persistedInvoice.EffectivePaidAmount);

        Assert.Equal(
            0m,
            persistedInvoice.BalanceAmount);

        Assert.Equal(
            TuitionInvoiceStatus.Paid,
            persistedInvoice.Status);

        // Debit ban đầu phải vẫn là 300.000đ.
        var debit =
            await db.StudentCreditTransactions
                .SingleAsync(x =>
                    x.TuitionInvoiceId == invoice.Id &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Debit);

        Assert.Equal(300_000m, debit.Amount);

        // Phần giảm 100.000đ phải được trả lại thành Credit.
        var returnedCredit =
            await db.StudentCreditTransactions
                .SingleAsync(x =>
                    x.TuitionInvoiceId == invoice.Id &&
                    x.TransactionType ==
                        StudentCreditTransactionType.Credit);

        Assert.Equal(100_000m, returnedCredit.Amount);
        Assert.Equal(studentId, returnedCredit.StudentId);
        Assert.NotNull(returnedCredit.TuitionAdjustmentId);

        // Tổng tác động Student Credit:
        // -300.000 + 100.000 = -200.000.
        var creditBalance =
            await new StudentCreditRepository(db)
                .GetBalanceAsync(
                    tenantId,
                    studentId);

        Assert.Equal(-200_000m, creditBalance);
    }
}