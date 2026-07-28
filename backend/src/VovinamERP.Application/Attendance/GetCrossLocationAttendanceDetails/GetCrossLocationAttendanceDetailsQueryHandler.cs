using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed class GetCrossLocationAttendanceDetailsQueryHandler
    : IRequestHandler<
        GetCrossLocationAttendanceDetailsQuery,
        GetCrossLocationAttendanceDetailsResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetCrossLocationAttendanceDetailsQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetCrossLocationAttendanceDetailsResult> Handle(
        GetCrossLocationAttendanceDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await _attendanceRepository
                .GetCrossLocationAttendanceDetailsAsync(
                    request.TenantId,
                    request.FromDate,
                    request.ToDate,
                    request.StudentId,
                    request.TrainingOrganizationId,
                    request.PageNumber,
                    request.PageSize,
                    request.SortBy,
                    request.Descending,
                    cancellationToken);

        var totalPages =
            result.TotalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    result.TotalCount /
                    (double)request.PageSize);

        return new GetCrossLocationAttendanceDetailsResult(
            result.Items,
            request.PageNumber,
            request.PageSize,
            result.TotalCount,
            totalPages);
    }
}