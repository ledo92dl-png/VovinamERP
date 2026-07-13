using MediatR;
using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.MarkStudentAttendance;

public sealed record MarkStudentAttendanceCommand(
    Guid TenantId,
    Guid AttendanceRecordId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    Guid MarkedByUserId,
    bool IsBackfilled,
    string? Note)
    : IRequest<MarkStudentAttendanceResult>;