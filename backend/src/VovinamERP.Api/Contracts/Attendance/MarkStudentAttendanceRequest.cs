using VovinamERP.Domain.Training;

namespace VovinamERP.Api.Contracts.Attendance;

public sealed record MarkStudentAttendanceRequest(
    Guid TenantId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    Guid MarkedByUserId,
    bool IsBackfilled,
    string? Note);