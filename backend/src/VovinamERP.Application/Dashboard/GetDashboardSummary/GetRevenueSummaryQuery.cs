using MediatR;

namespace VovinamERP.Application.Dashboard.GetRevenueSummary;

public sealed record GetRevenueSummaryQuery(
    Guid TenantId,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? OrganizationId)
    : IRequest<GetRevenueSummaryResult>;