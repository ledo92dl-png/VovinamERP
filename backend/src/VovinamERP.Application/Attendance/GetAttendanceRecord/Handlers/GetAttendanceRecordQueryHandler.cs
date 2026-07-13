using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetAttendanceRecord.Handlers;

public sealed class GetAttendanceRecordQueryHandler
    : IRequestHandler<GetAttendanceRecordQuery, GetAttendanceRecordResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetAttendanceRecordQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetAttendanceRecordResult> Handle(
        GetAttendanceRecordQuery request,
        CancellationToken cancellationToken)
    {
        var attendanceRecord =
            await _attendanceRepository.GetByIdAsync(
                request.AttendanceRecordId,
                request.TenantId,
                cancellationToken);

        if (attendanceRecord is null)
        {
            throw new InvalidOperationException(
                "Attendance record was not found.");
        }

        var details = attendanceRecord.Details
            .OrderBy(x => x.MarkedAt)
            .Select(x => new AttendanceDetailItem(
                x.Id,
                x.StudentId,
                x.Status,
                x.Method,
                x.Source,
                x.MarkedAt,
                x.MarkedByUserId,
                x.IsBackfilled,
                x.Note))
            .ToList();

        return new GetAttendanceRecordResult(
            attendanceRecord.Id,
            attendanceRecord.TenantId,
            attendanceRecord.TrainingSessionId,
            attendanceRecord.CreatedByUserId,
            details);
    }
}