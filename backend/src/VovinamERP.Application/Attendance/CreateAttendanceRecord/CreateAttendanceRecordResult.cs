namespace VovinamERP.Application.Attendance.CreateAttendanceRecord;

public sealed record CreateAttendanceRecordResult(
    Guid AttendanceRecordId,
    Guid TenantId,
    Guid TrainingSessionId,
    Guid CreatedByUserId);