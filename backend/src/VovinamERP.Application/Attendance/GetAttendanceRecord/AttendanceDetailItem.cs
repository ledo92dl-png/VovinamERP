using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.GetAttendanceRecord;

public sealed record AttendanceDetailItem(
    Guid AttendanceDetailId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    DateTimeOffset MarkedAt,
    Guid MarkedByUserId,
    bool IsBackfilled,
    string? Note);