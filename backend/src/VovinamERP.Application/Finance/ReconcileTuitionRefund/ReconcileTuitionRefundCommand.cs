using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ReconcileTuitionRefund;

public sealed record ReconcileTuitionRefundCommand(
    Guid TenantId,
    Guid TuitionRefundId,
    Guid ReconciledByUserId
) : IRequest<Result<ReconcileTuitionRefundResult>>;