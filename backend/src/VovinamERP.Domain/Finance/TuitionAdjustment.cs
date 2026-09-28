
using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Finance;

public sealed class TuitionAdjustment : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TuitionInvoiceId { get; private set; }

    public decimal PreviousPayableAmount { get; private set; }
    public decimal NewPayableAmount { get; private set; }
    public decimal AdjustmentAmount { get; private set; }
    public decimal SettlementAmount { get; private set; }

    public TuitionAdjustmentSettlementType SettlementType
    {
        get;
        private set;
    }

    public string Reason { get; private set; } = default!;
    public Guid ApprovedByUserId { get; private set; }
    public DateTime ApprovedAtUtc { get; private set; }

    private TuitionAdjustment() { }

    
public static Result<TuitionAdjustment> Create(
    Guid tenantId,
    Guid studentId,
    Guid tuitionInvoiceId,
    decimal previousPayableAmount,
    decimal newPayableAmount,
    decimal settlementAmount,
    TuitionAdjustmentSettlementType settlementType,
    string reason,
    Guid approvedByUserId)
    {
        if (tenantId == Guid.Empty ||
            studentId == Guid.Empty ||
            tuitionInvoiceId == Guid.Empty ||
            approvedByUserId == Guid.Empty)
        {
            return Result<TuitionAdjustment>.Failure(
                new Error(
                    "FIN_062",
                    "Required adjustment identifier is missing."));
        }

        if (previousPayableAmount < 0 ||
            newPayableAmount < 0 ||
            newPayableAmount >= previousPayableAmount)
        {
            return Result<TuitionAdjustment>.Failure(
                new Error(
                    "FIN_063",
                    "Adjustment amounts are invalid."));
        }

        if (settlementAmount < 0 ||
    settlementAmount > previousPayableAmount - newPayableAmount)
{
    return Result<TuitionAdjustment>.Failure(
        new Error(
            "FIN_080",
            "Settlement amount is invalid."));
}

        if (!Enum.IsDefined(settlementType))
        {
            return Result<TuitionAdjustment>.Failure(
                new Error(
                    "FIN_064",
                    "Adjustment settlement type is invalid."));
        }

        if (string.IsNullOrWhiteSpace(reason) ||
            reason.Trim().Length > 500)
        {
            return Result<TuitionAdjustment>.Failure(
                new Error(
                    "FIN_065",
                    "Adjustment reason is required and must not exceed 500 characters."));
        }

        return Result<TuitionAdjustment>.Success(
            new TuitionAdjustment
            {
                TenantId = tenantId,
                StudentId = studentId,
                TuitionInvoiceId = tuitionInvoiceId,
                PreviousPayableAmount = previousPayableAmount,
                NewPayableAmount = newPayableAmount,
                AdjustmentAmount =
                    previousPayableAmount - newPayableAmount,
                SettlementType = settlementType,
                SettlementAmount = settlementAmount,
                Reason = reason.Trim(),
                ApprovedByUserId = approvedByUserId,
                ApprovedAtUtc = DateTime.UtcNow
            });
    }
}