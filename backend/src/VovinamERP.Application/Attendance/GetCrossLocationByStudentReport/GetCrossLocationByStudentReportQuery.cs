using MediatR;

namespace VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;

public sealed record GetCrossLocationByStudentReportQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate)
    : IRequest<GetCrossLocationByStudentReportResult>;