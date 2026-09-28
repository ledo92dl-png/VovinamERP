
using MediatR;
using VovinamERP.Domain.Finance;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.AdjustPaidTuition;

public sealed record AdjustPaidTuitionCommand(
    Guid TenantId,
    Guid TuitionInvoiceId,
    decimal NewSpecialDiscountAmount,
    TuitionAdjustmentSettlementType SettlementType,
    string Reason,
    Guid ApprovedByUserId
) : IRequest<Result<AdjustPaidTuitionResult>>;