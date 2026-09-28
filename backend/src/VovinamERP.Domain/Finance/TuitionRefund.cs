
using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class TuitionRefund : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TuitionInvoiceId { get; private set; }
    public Guid TuitionAdjustmentId { get; private set; }

    public decimal Amount { get; private set; }
    public TuitionRefundStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateOnly? RefundedDate { get; private set; }
    public Guid? RefundedByUserId { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? EvidenceImageUrl { get; private set; }

    public Guid? ReconciledByUserId { get; private set; }
    public DateTime? ReconciledAtUtc { get; private set; }

    public Guid? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    private TuitionRefund() { }

    public static Result<TuitionRefund> Create(
        Guid tenantId,
        Guid studentId,
        Guid tuitionInvoiceId,
        Guid tuitionAdjustmentId,
        decimal amount)
    {
        if (tenantId == Guid.Empty ||
            studentId == Guid.Empty ||
            tuitionInvoiceId == Guid.Empty ||
            tuitionAdjustmentId == Guid.Empty)
        {
            return Result<TuitionRefund>.Failure(
                new Error(
                    "FIN_066",
                    "Required refund identifier is missing."));
        }

        if (amount <= 0)
        {
            return Result<TuitionRefund>.Failure(
                FinanceErrors.AmountMustBePositive);
        }

        return Result<TuitionRefund>.Success(
            new TuitionRefund
            {
                TenantId = tenantId,
                StudentId = studentId,
                TuitionInvoiceId = tuitionInvoiceId,
                TuitionAdjustmentId = tuitionAdjustmentId,
                Amount = amount,
                Status = TuitionRefundStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            });
    }

    public Result ConfirmRefund(
        DateOnly refundedDate,
        Guid refundedByUserId,
        PaymentMethod paymentMethod,
        string? transactionReference,
        string? evidenceImageUrl)
    {
        if (Status != TuitionRefundStatus.Pending)
        {
            return Result.Failure(
                new Error(
                    "FIN_067",
                    "Only pending refunds can be confirmed."));
        }

        if (refundedByUserId == Guid.Empty ||
            !Enum.IsDefined(paymentMethod))
        {
            return Result.Failure(
                new Error(
                    "FIN_068",
                    "Refund operator or payment method is invalid."));
        }

        if (paymentMethod !=
        global::VovinamERP.Domain.Finance.PaymentMethod.Cash &&
    string.IsNullOrWhiteSpace(transactionReference))
        {
            return Result.Failure(
                new Error(
                    "FIN_069",
                    "Transaction reference is required for non-cash refunds."));
        }

        if (evidenceImageUrl?.Trim().Length > 2000 ||
            transactionReference?.Trim().Length > 200)
        {
            return Result.Failure(
                new Error(
                    "FIN_070",
                    "Refund evidence or transaction reference is too long."));
        }

        RefundedDate = refundedDate;
        RefundedByUserId = refundedByUserId;
        PaymentMethod = paymentMethod;
        TransactionReference = transactionReference?.Trim();
        EvidenceImageUrl = evidenceImageUrl?.Trim();

        Status = TuitionRefundStatus.Refunded;

        return Result.Success();
    }

    public Result Reconcile(Guid reconciledByUserId)
    {
        if (Status != TuitionRefundStatus.Refunded)
        {
            return Result.Failure(
                new Error(
                    "FIN_071",
                    "Only completed refunds can be reconciled."));
        }

        if (reconciledByUserId == Guid.Empty)
        {
            return Result.Failure(
                new Error(
                    "FIN_072",
                    "Reconciliation user is required."));
        }

        ReconciledByUserId = reconciledByUserId;
        ReconciledAtUtc = DateTime.UtcNow;
        Status = TuitionRefundStatus.Reconciled;

        return Result.Success();
    }

    public Result Cancel(
        Guid cancelledByUserId,
        string reason)
    {
        if (Status != TuitionRefundStatus.Pending)
        {
            return Result.Failure(
                new Error(
                    "FIN_073",
                    "Only pending refunds can be cancelled."));
        }

        if (cancelledByUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(reason) ||
            reason.Trim().Length > 500)
        {
            return Result.Failure(
                new Error(
                    "FIN_074",
                    "Cancellation user and valid reason are required."));
        }

        CancelledByUserId = cancelledByUserId;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = reason.Trim();

        Status = TuitionRefundStatus.Cancelled;

        return Result.Success();
    }
}