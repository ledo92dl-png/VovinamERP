using MediatR;
using VovinamERP.Application.Dashboard.Common;

namespace VovinamERP.Application.Dashboard.GetStudentsByBelt;

public sealed class GetStudentsByBeltQueryHandler
    : IRequestHandler<
        GetStudentsByBeltQuery,
        GetStudentsByBeltResult>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetStudentsByBeltQueryHandler(
        IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<GetStudentsByBeltResult> Handle(
        GetStudentsByBeltQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await _dashboardRepository.GetStudentsByBeltAsync(
                request.TenantId,
                request.OrganizationId,
                cancellationToken);

        return new GetStudentsByBeltResult(
            result.Items,
            result.TotalStudents);
    }
}