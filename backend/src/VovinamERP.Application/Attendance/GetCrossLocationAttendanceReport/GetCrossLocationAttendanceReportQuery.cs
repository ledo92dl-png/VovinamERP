using MediatR;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceReport;

public sealed record GetCrossLocationAttendanceReportQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate)
    : IRequest<GetCrossLocationAttendanceReportResult>;