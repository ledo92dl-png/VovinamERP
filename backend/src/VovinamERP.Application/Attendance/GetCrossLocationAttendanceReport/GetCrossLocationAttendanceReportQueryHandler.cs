using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceReport;

public sealed class GetCrossLocationAttendanceReportQueryHandler
    : IRequestHandler<
        GetCrossLocationAttendanceReportQuery,
        GetCrossLocationAttendanceReportResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetCrossLocationAttendanceReportQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetCrossLocationAttendanceReportResult> Handle(
        GetCrossLocationAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var summary =
            await _attendanceRepository
                .GetCrossLocationSummaryAsync(
                    request.TenantId,
                    request.FromDate,
                    request.ToDate,
                    cancellationToken);

        var normalAttendances =
            summary.TotalAttendances -
            summary.CrossLocationAttendances;

        var crossLocationRate =
            summary.TotalAttendances == 0
                ? 0m
                : Math.Round(
                    summary.CrossLocationAttendances * 100m /
                    summary.TotalAttendances,
                    2);

        return new GetCrossLocationAttendanceReportResult(
            summary.TotalAttendances,
            summary.CrossLocationAttendances,
            normalAttendances,
            crossLocationRate);
    }
}