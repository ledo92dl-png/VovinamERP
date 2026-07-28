using MediatR;

namespace VovinamERP.Application.Dashboard.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    Guid TenantId,
    DateOnly ReportDate)
    : IRequest<GetDashboardSummaryResult>;