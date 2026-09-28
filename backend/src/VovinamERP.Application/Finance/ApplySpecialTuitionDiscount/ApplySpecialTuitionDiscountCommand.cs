using MediatR;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.ApplySpecialTuitionDiscount;

public sealed record ApplySpecialTuitionDiscountCommand(
    Guid TenantId,
    Guid StudentId,
    Guid TuitionInvoiceId,
    TuitionSpecialDiscountType DiscountType,
    decimal? DiscountValue,
    string Reason,
    Guid ApprovedByUserId)
    : IRequest<ApplySpecialTuitionDiscountResult>;