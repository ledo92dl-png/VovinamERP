using MediatR;
using VovinamERP.Application.Dashboard.Common;

namespace VovinamERP.Application.Dashboard.GetAttendanceTrend;

public sealed class GetAttendanceTrendQueryHandler
    : IRequestHandler<
        GetAttendanceTrendQuery,
        GetAttendanceTrendResult>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetAttendanceTrendQueryHandler(
        IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<GetAttendanceTrendResult> Handle(
        GetAttendanceTrendQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await _dashboardRepository.GetAttendanceTrendAsync(
                request.TenantId,
                request.FromDate,
                request.ToDate,
                request.OrganizationId,
                cancellationToken);

        return new GetAttendanceTrendResult(
            items,
            request.FromDate,
            request.ToDate);
    }
}