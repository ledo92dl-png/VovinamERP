using MediatR;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.ConfirmTuitionRefund;

public sealed record ConfirmTuitionRefundCommand(
    Guid TenantId,
    Guid TuitionRefundId,
    DateOnly RefundedDate,
    Guid RefundedByUserId,
    PaymentMethod PaymentMethod,
    string? TransactionReference,
    string? EvidenceImageUrl
) : IRequest<Result<ConfirmTuitionRefundResult>>;