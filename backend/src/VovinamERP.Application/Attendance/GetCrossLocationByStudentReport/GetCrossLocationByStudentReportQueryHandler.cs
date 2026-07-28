using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;

public sealed class GetCrossLocationByStudentReportQueryHandler
    : IRequestHandler<
        GetCrossLocationByStudentReportQuery,
        GetCrossLocationByStudentReportResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetCrossLocationByStudentReportQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetCrossLocationByStudentReportResult> Handle(
        GetCrossLocationByStudentReportQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await _attendanceRepository
                .GetCrossLocationByStudentAsync(
                    request.TenantId,
                    request.FromDate,
                    request.ToDate,
                    cancellationToken);

        return new GetCrossLocationByStudentReportResult(
            items);
    }
}