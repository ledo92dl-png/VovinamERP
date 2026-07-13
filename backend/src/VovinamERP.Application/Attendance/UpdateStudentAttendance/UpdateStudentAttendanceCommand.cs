using MediatR;
using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.UpdateStudentAttendance;

public sealed record UpdateStudentAttendanceCommand(
    Guid AttendanceRecordId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source,
    Guid MarkedByUserId,
    bool IsBackfilled,
    string? Note)
    : IRequest<UpdateStudentAttendanceResult>;