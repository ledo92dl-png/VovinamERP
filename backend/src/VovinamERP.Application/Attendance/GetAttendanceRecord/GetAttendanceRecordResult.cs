namespace VovinamERP.Application.Attendance.GetAttendanceRecord;

public sealed record GetAttendanceRecordResult(
    Guid AttendanceRecordId,
    Guid TenantId,
    Guid TrainingSessionId,
    Guid CreatedByUserId,
    IReadOnlyList<AttendanceDetailItem> Details);