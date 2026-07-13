using VovinamERP.Domain.Training;

namespace VovinamERP.Api.Contracts.Attendance;

public sealed record UpdateStudentAttendanceRequest(
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    Guid MarkedByUserId,
    bool IsBackfilled,
    string? Note);