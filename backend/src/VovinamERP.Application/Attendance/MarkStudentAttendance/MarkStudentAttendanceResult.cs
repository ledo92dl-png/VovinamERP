using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.MarkStudentAttendance;

public sealed record MarkStudentAttendanceResult(
    Guid AttendanceRecordId,
    Guid AttendanceDetailId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    DateTimeOffset MarkedAt,
    bool IsBackfilled);