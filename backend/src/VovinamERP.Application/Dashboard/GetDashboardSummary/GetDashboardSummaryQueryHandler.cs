using MediatR;
using VovinamERP.Application.Dashboard.Common;

namespace VovinamERP.Application.Dashboard.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<
        GetDashboardSummaryQuery,
        GetDashboardSummaryResult>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryQueryHandler(
        IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<GetDashboardSummaryResult> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var activeStudents =
            await _dashboardRepository.CountActiveStudentsAsync(
                request.TenantId,
                cancellationToken);

        var activeInstructors =
            await _dashboardRepository.CountActiveInstructorsAsync(
                request.TenantId,
                cancellationToken);

        var activeOrganizations =
            await _dashboardRepository.CountActiveOrganizationsAsync(
                request.TenantId,
                cancellationToken);

        var trainingSessions =
            await _dashboardRepository.CountTrainingSessionsAsync(
                request.TenantId,
                request.ReportDate,
                cancellationToken);

        var attendanceCount =
            await _dashboardRepository.CountAttendancesAsync(
                request.TenantId,
                request.ReportDate,
                cancellationToken);

        var crossLocationAttendanceCount =
            await _dashboardRepository
                .CountCrossLocationAttendancesAsync(
                    request.TenantId,
                    request.ReportDate,
                    cancellationToken);

        return new GetDashboardSummaryResult(
            request.TenantId,
            request.ReportDate,
            activeStudents,
            activeInstructors,
            activeOrganizations,
            trainingSessions,
            attendanceCount,
            crossLocationAttendanceCount);
    }
}