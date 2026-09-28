
using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.AdjustPaidTuition;

public sealed class AdjustPaidTuitionCommandHandler
    : IRequestHandler<
        AdjustPaidTuitionCommand,
        Result<AdjustPaidTuitionResult>>
{
    private readonly ITuitionInvoiceRepository _invoiceRepository;
    private readonly ITuitionAdjustmentRepository _adjustmentRepository;
    private readonly ITuitionRefundRepository _refundRepository;
    private readonly IStudentCreditRepository _creditRepository;
    private readonly ITuitionAdjustmentTransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustPaidTuitionCommandHandler(
        ITuitionInvoiceRepository invoiceRepository,
        ITuitionAdjustmentRepository adjustmentRepository,
        ITuitionRefundRepository refundRepository,
        IStudentCreditRepository creditRepository,
        ITuitionAdjustmentTransaction transaction,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _adjustmentRepository = adjustmentRepository;
        _refundRepository = refundRepository;
        _creditRepository = creditRepository;
        _transaction = transaction;
        _unitOfWork = unitOfWork;
    }

    
public async Task<Result<AdjustPaidTuitionResult>> Handle(
    AdjustPaidTuitionCommand request,
    CancellationToken cancellationToken)
{
    return await _transaction.ExecuteWithInvoiceLockAsync(
        request.TenantId,
        request.TuitionInvoiceId,
        async ct =>
        {
            // Tải hóa đơn sau khi đã lấy khóa.
            var invoice = await _invoiceRepository.GetByIdAsync(
                request.TenantId,
                request.TuitionInvoiceId,
                ct);

            if (invoice is null)
            {
                return Result<AdjustPaidTuitionResult>.Failure(
    new Error(
        "FIN_084",
        "Tuition invoice was not found."));
                
            }

            var previousPayableAmount =
                invoice.Amount
                - invoice.DiscountAmount
                - invoice.SpecialDiscountAmount;

            var newPayableAmount =
                invoice.Amount
                - invoice.DiscountAmount
                - request.NewSpecialDiscountAmount;

            if (request.NewSpecialDiscountAmount
                    <= invoice.SpecialDiscountAmount
                || newPayableAmount < 0)
            {
                return Result<AdjustPaidTuitionResult>.Failure(
                    new Error(
                        "FIN_085",
                        "The new special discount is invalid."));
                
            }

            var adjustmentAmount =
                previousPayableAmount - newPayableAmount;

            var settlementAmount =
                invoice.GetRequiredSettlementAmount(
                    request.NewSpecialDiscountAmount);
            
if (invoice.ReallocatedAmount < 0 ||
    invoice.ReallocatedAmount > invoice.PaidAmount)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        new Error(
            "FIN_089",
            "Invoice payment allocation is inconsistent."));
    
}


var previousAdjustments =
    await _adjustmentRepository.GetByInvoiceIdAsync(
        request.TenantId,
        invoice.Id,
        ct);

var totalPreviouslySettled =
    previousAdjustments.Sum(x => x.SettlementAmount);

if (totalPreviouslySettled != invoice.ReallocatedAmount)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        new Error(
            "FIN_091",
            "Invoice reallocated amount does not match adjustment history."));
    
}

var previousSettlementsValid =
    await ValidatePreviousSettlementsAsync(
        request.TenantId,
        invoice.StudentId,
        invoice.Id,
        previousAdjustments,
        ct);

if (!previousSettlementsValid)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        new Error(
            "FIN_092",
            "Previous tuition settlements are inconsistent. " +
            "Manual reconciliation is required."));
    
}
var effectivePaidAmount =
    invoice.PaidAmount - invoice.ReallocatedAmount;

if (settlementAmount >
    Math.Max(0m, effectivePaidAmount - newPayableAmount))
{
    return Result<AdjustPaidTuitionResult>.Failure(
        new Error(
            "FIN_090",
            "Settlement exceeds the current overpayment."));
    
}
if (settlementAmount > 0)
{
    var paymentSources =
        await GetOriginalPaymentSourcesAsync(
            request.TenantId,
            invoice.StudentId,
            invoice.Id,
            invoice.PaidAmount,
            ct);

    if (request.SettlementType ==
            VovinamERP.Domain.Finance
                .TuitionAdjustmentSettlementType.Refund
        && paymentSources.CreditApplied > 0)
    {
        return Result<AdjustPaidTuitionResult>.Failure(
            new Error(
                "FIN_088",
                "This invoice used Student Credit. " +
                "Select Student Credit settlement; " +
                "mixed-source refunds are not supported yet."));
        
    }
}

            // Chưa thay đổi dữ liệu trong bước này.
            
var adjustmentResult =
    VovinamERP.Domain.Finance.TuitionAdjustment.Create(
        request.TenantId,
        invoice.StudentId,
        invoice.Id,
        previousPayableAmount,
        newPayableAmount,
        settlementAmount,
        request.SettlementType,
        request.Reason,
        request.ApprovedByUserId);

if (adjustmentResult.IsFailure ||
    adjustmentResult.Value is null)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        adjustmentResult.Error);
    
}

var adjustment = adjustmentResult.Value;

var discountResult = invoice.ApplyPostPaymentDiscount(
    request.NewSpecialDiscountAmount,
    request.Reason,
    request.ApprovedByUserId);

if (discountResult.IsFailure)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        discountResult.Error);
    
}

if (settlementAmount > 0)
{
    var reallocationResult = invoice.ReallocateOverpayment(
        settlementAmount,
        request.ApprovedByUserId);

    if (reallocationResult.IsFailure)
    {
        return Result<AdjustPaidTuitionResult>.Failure(
            reallocationResult.Error);
        
    }
}

Guid? studentCreditTransactionId = null;

if (settlementAmount > 0 &&
    request.SettlementType ==
        VovinamERP.Domain.Finance
            .TuitionAdjustmentSettlementType.StudentCredit)
{
    var creditResult =
        VovinamERP.Domain.Finance.StudentCreditTransaction.Create(
            request.TenantId,
            invoice.StudentId,
            VovinamERP.Domain.Finance
                .StudentCreditTransactionType.Credit,
            settlementAmount,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            null,
            invoice.Id,
            $"Tuition adjustment {adjustment.Id}");

    if (creditResult.IsFailure ||
        creditResult.Value is null)
    {
        return Result<AdjustPaidTuitionResult>.Failure(
            creditResult.Error);
        
    }

    var creditTransaction = creditResult.Value;

    var linkResult =
        creditTransaction.LinkToTuitionAdjustment(
            adjustment.Id);

    if (linkResult.IsFailure)
    {
        return Result<AdjustPaidTuitionResult>.Failure(
            linkResult.Error);
        
    }

    await _creditRepository.AddAsync(
        creditTransaction,
        ct);

    studentCreditTransactionId = creditTransaction.Id;
}
    
Guid? tuitionRefundId = null;

if (settlementAmount > 0 &&
    request.SettlementType ==
        VovinamERP.Domain.Finance
            .TuitionAdjustmentSettlementType.Refund)
{
    var refundResult =
        VovinamERP.Domain.Finance.TuitionRefund.Create(
            request.TenantId,
            invoice.StudentId,
            invoice.Id,
            adjustment.Id,
            settlementAmount);

    if (refundResult.IsFailure ||
    refundResult.Value is null)
{
    return Result<AdjustPaidTuitionResult>.Failure(
        refundResult.Error);
}

    var refund = refundResult.Value;

    await _refundRepository.AddAsync(
        refund,
        ct);

    tuitionRefundId = refund.Id;
}


// Chưa lưu vào cơ sở dữ liệu.
// Các bước tiếp theo sẽ cập nhật hóa đơn và tạo
// Student Credit hoặc phiếu hoàn tiền trước khi lưu.


await _adjustmentRepository.AddAsync(
    adjustment,
    ct);

// Lưu hóa đơn, lịch sử điều chỉnh và chứng từ
// Student Credit hoặc hoàn tiền trong cùng giao dịch.
await _unitOfWork.SaveChangesAsync(ct);

return Result<AdjustPaidTuitionResult>.Success(
    new AdjustPaidTuitionResult(
        adjustment.Id,
        invoice.Id,
        invoice.StudentId,
        previousPayableAmount,
        newPayableAmount,
        adjustmentAmount,
        settlementAmount,
        invoice.BalanceAmount,
        request.SettlementType,
        studentCreditTransactionId,
        tuitionRefundId,
        tuitionRefundId.HasValue
            ? VovinamERP.Domain.Finance.TuitionRefundStatus.Pending
            : null));

        },
        cancellationToken);

    }


private async Task<(decimal CreditApplied, decimal CashPaid)>
    GetOriginalPaymentSourcesAsync(
        Guid tenantId,
        Guid studentId,
        Guid tuitionInvoiceId,
        decimal paidAmount,
        CancellationToken cancellationToken)
{
    var transactions = await _creditRepository.GetByInvoiceIdAsync(
        tenantId,
        studentId,
        tuitionInvoiceId,
        cancellationToken);

    var creditApplied = transactions
        .Where(x =>
            x.TransactionType ==
                VovinamERP.Domain.Finance.StudentCreditTransactionType.Debit)
        .Sum(x => x.Amount);

    var cashPaid = paidAmount - creditApplied;

    if (cashPaid < 0)
    {
        throw new InvalidOperationException(
            "Student Credit applied exceeds the invoice paid amount.");
    }

    return (creditApplied, cashPaid);
}

private async Task<bool> ValidatePreviousSettlementsAsync(
    Guid tenantId,
    Guid studentId,
    Guid tuitionInvoiceId,
    IReadOnlyCollection<
        VovinamERP.Domain.Finance.TuitionAdjustment> adjustments,
    CancellationToken cancellationToken)
{
    var creditTransactions =
        await _creditRepository.GetByInvoiceIdAsync(
            tenantId,
            studentId,
            tuitionInvoiceId,
            cancellationToken);

    var refunds =
        await _refundRepository.GetByInvoiceIdAsync(
            tenantId,
            tuitionInvoiceId,
            cancellationToken);

    foreach (var adjustment in adjustments)
    {
        var linkedCredits = creditTransactions
            .Where(x =>
                x.TuitionAdjustmentId == adjustment.Id)
            .ToList();

        var linkedRefunds = refunds
            .Where(x =>
                x.TuitionAdjustmentId == adjustment.Id)
            .ToList();

        if (adjustment.SettlementAmount == 0)
        {
            if (linkedCredits.Count != 0 ||
                linkedRefunds.Count != 0)
                return false;

            continue;
        }

        if (adjustment.SettlementType ==
            VovinamERP.Domain.Finance
                .TuitionAdjustmentSettlementType.StudentCredit)
        {
            if (linkedCredits.Count != 1 ||
                linkedRefunds.Count != 0 ||
                linkedCredits[0].TransactionType !=
                    VovinamERP.Domain.Finance
                        .StudentCreditTransactionType.Credit ||
                linkedCredits[0].Amount !=
                    adjustment.SettlementAmount)
                return false;
        }
        else if (adjustment.SettlementType ==
            VovinamERP.Domain.Finance
                .TuitionAdjustmentSettlementType.Refund)
        {
            if (linkedCredits.Count != 0 ||
                linkedRefunds.Count != 1 ||
                linkedRefunds[0].Amount !=
                    adjustment.SettlementAmount ||
                linkedRefunds[0].Status ==
                    VovinamERP.Domain.Finance
                        .TuitionRefundStatus.Cancelled)
                return false;
        }
        else
        {
            return false;
        }
    }

    return true;
}
}
