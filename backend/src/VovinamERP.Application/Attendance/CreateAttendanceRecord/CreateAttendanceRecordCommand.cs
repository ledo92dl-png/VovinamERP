using MediatR;

namespace VovinamERP.Application.Attendance.CreateAttendanceRecord;

public sealed record CreateAttendanceRecordCommand(
    Guid TenantId,
    Guid TrainingSessionId,
    Guid CreatedByUserId)
    : IRequest<CreateAttendanceRecordResult>;