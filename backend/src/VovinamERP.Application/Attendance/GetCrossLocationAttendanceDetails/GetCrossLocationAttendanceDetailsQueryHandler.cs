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
        var items =
            await _attendanceRepository
                .GetCrossLocationAttendanceDetailsAsync(
                    request.TenantId,
                    request.FromDate,
                    request.ToDate,
                    request.StudentId,
                    request.TrainingOrganizationId,
                    cancellationToken);

        return new GetCrossLocationAttendanceDetailsResult(items);
    }
}