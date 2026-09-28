
using VovinamERP.Domain.Finance;

namespace VovinamERP.Tests;

public class TuitionRefundTests
{
    private static TuitionRefund CreateRefund()
    {
        var result = TuitionRefund.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            50_000m);

        Assert.True(result.IsSuccess);

        return Assert.IsType<TuitionRefund>(result.Value);
    }

    [Fact]
    public void NewRefund_ShouldBePending()
    {
        var refund = CreateRefund();

        Assert.Equal(50_000m, refund.Amount);
        Assert.Equal(
            TuitionRefundStatus.Pending,
            refund.Status);

        Assert.Null(refund.RefundedDate);
        Assert.Null(refund.ReconciledAtUtc);
    }

    [Fact]
    public void Refund_ShouldProgressFromPendingToReconciled()
    {
        var refund = CreateRefund();
        var operatorId = Guid.NewGuid();
        var accountantId = Guid.NewGuid();

        var confirmResult = refund.ConfirmRefund(
            new DateOnly(2027, 1, 6),
            operatorId,
            PaymentMethod.Cash,
            null,
            null);

        Assert.True(confirmResult.IsSuccess);
        Assert.Equal(
            TuitionRefundStatus.Refunded,
            refund.Status);

        Assert.Equal(operatorId, refund.RefundedByUserId);
        Assert.Equal(
            new DateOnly(2027, 1, 6),
            refund.RefundedDate);

        var reconcileResult =
            refund.Reconcile(accountantId);

        Assert.True(reconcileResult.IsSuccess);
        Assert.Equal(
            TuitionRefundStatus.Reconciled,
            refund.Status);

        Assert.Equal(
            accountantId,
            refund.ReconciledByUserId);

        Assert.NotNull(refund.ReconciledAtUtc);
    }

    [Fact]
    public void Refund_ShouldNotBeConfirmedTwice()
    {
        var refund = CreateRefund();

        var firstResult = refund.ConfirmRefund(
            new DateOnly(2027, 1, 6),
            Guid.NewGuid(),
            PaymentMethod.Cash,
            null,
            null);

        var secondResult = refund.ConfirmRefund(
            new DateOnly(2027, 1, 7),
            Guid.NewGuid(),
            PaymentMethod.Cash,
            null,
            null);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);

        Assert.Equal(
            new DateOnly(2027, 1, 6),
            refund.RefundedDate);
    }

    [Fact]
    public void PendingRefund_ShouldNotBeReconciled()
    {
        var refund = CreateRefund();

        var result = refund.Reconcile(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(
            TuitionRefundStatus.Pending,
            refund.Status);

        Assert.Null(refund.ReconciledAtUtc);
    }

    [Fact]
    public void CancelledRefund_ShouldNotBeConfirmed()
    {
        var refund = CreateRefund();

        var cancelResult = refund.Cancel(
            Guid.NewGuid(),
            "Refund request cancelled during review");

        Assert.True(cancelResult.IsSuccess);

        var confirmResult = refund.ConfirmRefund(
            new DateOnly(2027, 1, 6),
            Guid.NewGuid(),
            PaymentMethod.Cash,
            null,
            null);

        Assert.True(confirmResult.IsFailure);
        Assert.Equal(
            TuitionRefundStatus.Cancelled,
            refund.Status);
    }
}