using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ReconcileTuitionRefund;

public sealed class ReconcileTuitionRefundCommandHandler
    : IRequestHandler<
        ReconcileTuitionRefundCommand,
        Result<ReconcileTuitionRefundResult>>
{
    private readonly ITuitionRefundRepository _refundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReconcileTuitionRefundCommandHandler(
        ITuitionRefundRepository refundRepository,
        IUnitOfWork unitOfWork)
    {
        _refundRepository = refundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReconcileTuitionRefundResult>> Handle(
        ReconcileTuitionRefundCommand request,
        CancellationToken cancellationToken)
    {
        var refund =
            await _refundRepository.GetByIdAsync(
                request.TenantId,
                request.TuitionRefundId,
                cancellationToken);

        if (refund is null)
        {
            return Result<ReconcileTuitionRefundResult>.Failure(
                new Error(
                    "FIN_094",
                    "Tuition refund was not found."));
        }

        var reconcileResult =
            refund.Reconcile(
                request.ReconciledByUserId);

        if (reconcileResult.IsFailure)
        {
            return Result<ReconcileTuitionRefundResult>.Failure(
                reconcileResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<ReconcileTuitionRefundResult>.Success(
            new ReconcileTuitionRefundResult(
                refund.Id,
                refund.TuitionAdjustmentId,
                refund.TuitionInvoiceId,
                refund.StudentId,
                refund.Amount,
                refund.Status,
                refund.ReconciledByUserId!.Value,
                refund.ReconciledAtUtc!.Value));
    }
}