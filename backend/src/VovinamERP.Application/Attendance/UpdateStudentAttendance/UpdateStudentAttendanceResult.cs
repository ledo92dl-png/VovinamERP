namespace VovinamERP.Application.Attendance.UpdateStudentAttendance;

public sealed record UpdateStudentAttendanceResult(
    Guid AttendanceRecordId,
    Guid StudentId,
    string Status,
    string Method,
    string Source,
    DateTime UpdatedAt);