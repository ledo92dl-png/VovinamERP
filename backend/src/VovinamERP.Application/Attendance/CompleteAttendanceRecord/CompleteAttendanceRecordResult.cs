using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.CompleteAttendanceRecord;

public sealed record CompleteAttendanceRecordResult(
    Guid AttendanceRecordId,
    Guid TrainingSessionId,
    AttendanceRecordStatus Status,
    DateTimeOffset CompletedAt,
    Guid CompletedByUserId);