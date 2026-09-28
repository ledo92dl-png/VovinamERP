using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ConfirmTuitionRefund;

public sealed class ConfirmTuitionRefundCommandHandler
    : IRequestHandler<
        ConfirmTuitionRefundCommand,
        Result<ConfirmTuitionRefundResult>>
{
    private readonly ITuitionRefundRepository _refundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmTuitionRefundCommandHandler(
        ITuitionRefundRepository refundRepository,
        IUnitOfWork unitOfWork)
    {
        _refundRepository = refundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConfirmTuitionRefundResult>> Handle(
        ConfirmTuitionRefundCommand request,
        CancellationToken cancellationToken)
    {
        var refund =
            await _refundRepository.GetByIdAsync(
                request.TenantId,
                request.TuitionRefundId,
                cancellationToken);

        if (refund is null)
        {
            return Result<ConfirmTuitionRefundResult>.Failure(
                new Error(
                    "FIN_093",
                    "Tuition refund was not found."));
        }

        var confirmResult =
            refund.ConfirmRefund(
                request.RefundedDate,
                request.RefundedByUserId,
                request.PaymentMethod,
                request.TransactionReference,
                request.EvidenceImageUrl);

        if (confirmResult.IsFailure)
        {
            return Result<ConfirmTuitionRefundResult>.Failure(
                confirmResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<ConfirmTuitionRefundResult>.Success(
            new ConfirmTuitionRefundResult(
                refund.Id,
                refund.TuitionAdjustmentId,
                refund.TuitionInvoiceId,
                refund.StudentId,
                refund.Amount,
                refund.Status,
                refund.RefundedDate!.Value,
                refund.RefundedByUserId!.Value,
                refund.PaymentMethod!.Value,
                refund.TransactionReference,
                refund.EvidenceImageUrl));
    }
}