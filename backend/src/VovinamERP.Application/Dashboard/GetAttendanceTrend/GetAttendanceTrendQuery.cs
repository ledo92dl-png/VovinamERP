using MediatR;

namespace VovinamERP.Application.Dashboard.GetAttendanceTrend;

public sealed record GetAttendanceTrendQuery(
    Guid TenantId,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? OrganizationId)
    : IRequest<GetAttendanceTrendResult>;