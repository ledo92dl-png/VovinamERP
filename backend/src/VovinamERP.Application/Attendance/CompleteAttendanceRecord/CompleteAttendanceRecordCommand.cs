using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.CompleteAttendanceRecord;

public sealed record CompleteAttendanceRecordCommand(
    Guid TenantId,
    Guid AttendanceRecordId,
    Guid CompletedByUserId)
    : IRequest<Result<CompleteAttendanceRecordResult>>;