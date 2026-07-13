namespace VovinamERP.Application.Attendance.GetAttendanceRecords;

public sealed record AttendanceRecordListItem(
    Guid AttendanceRecordId,
    Guid TenantId,
    Guid TrainingSessionId,
    Guid CreatedByUserId,
    int TotalStudents);