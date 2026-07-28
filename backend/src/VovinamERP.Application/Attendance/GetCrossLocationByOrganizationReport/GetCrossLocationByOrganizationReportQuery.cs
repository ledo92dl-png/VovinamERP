using MediatR;

namespace VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

public sealed record GetCrossLocationByOrganizationReportQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate)
    : IRequest<GetCrossLocationByOrganizationReportResult>;