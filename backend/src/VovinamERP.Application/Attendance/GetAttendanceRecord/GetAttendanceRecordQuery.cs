using MediatR;

namespace VovinamERP.Application.Attendance.GetAttendanceRecord;

public sealed record GetAttendanceRecordQuery(
    Guid AttendanceRecordId,
    Guid TenantId)
    : IRequest<GetAttendanceRecordResult>;