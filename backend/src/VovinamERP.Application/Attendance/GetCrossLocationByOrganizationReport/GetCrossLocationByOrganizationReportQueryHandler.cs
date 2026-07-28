using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

public sealed class GetCrossLocationByOrganizationReportQueryHandler
    : IRequestHandler<
        GetCrossLocationByOrganizationReportQuery,
        GetCrossLocationByOrganizationReportResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetCrossLocationByOrganizationReportQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetCrossLocationByOrganizationReportResult> Handle(
        GetCrossLocationByOrganizationReportQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await _attendanceRepository
                .GetCrossLocationByOrganizationAsync(
                    request.TenantId,
                    request.FromDate,
                    request.ToDate,
                    cancellationToken);

        return new GetCrossLocationByOrganizationReportResult(items);
    }
}