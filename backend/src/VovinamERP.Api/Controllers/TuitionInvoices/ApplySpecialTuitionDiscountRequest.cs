using VovinamERP.Domain.Finance;

namespace VovinamERP.Api.Controllers.TuitionInvoices;

public sealed record ApplySpecialTuitionDiscountRequest(
    Guid TenantId,
    Guid StudentId,
    TuitionSpecialDiscountType DiscountType,
    decimal? DiscountValue,
    string Reason,
    Guid ApprovedByUserId);