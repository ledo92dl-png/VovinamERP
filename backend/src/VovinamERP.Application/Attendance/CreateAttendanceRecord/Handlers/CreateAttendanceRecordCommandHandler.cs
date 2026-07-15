using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.CreateAttendanceRecord.Handlers;

public sealed class CreateAttendanceRecordCommandHandler
    : IRequestHandler<CreateAttendanceRecordCommand, CreateAttendanceRecordResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public CreateAttendanceRecordCommandHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<CreateAttendanceRecordResult> Handle(
        CreateAttendanceRecordCommand request,
        CancellationToken cancellationToken)
    {
        var attendanceResult =
            AttendanceRecord.Create(
                request.TenantId,
                request.TrainingSessionId,
                request.CreatedByUserId);

        if (attendanceResult.IsFailure || attendanceResult.Value is null)
        {
            throw new InvalidOperationException(
    attendanceResult.Error.Message);
        }

        var attendanceRecord = attendanceResult.Value;

        await _attendanceRepository.AddRecordAsync(
            attendanceRecord,
            cancellationToken);

        return new CreateAttendanceRecordResult(
            attendanceRecord.Id,
            attendanceRecord.TenantId,
            attendanceRecord.TrainingSessionId,
            attendanceRecord.CreatedByUserId);
    }
}