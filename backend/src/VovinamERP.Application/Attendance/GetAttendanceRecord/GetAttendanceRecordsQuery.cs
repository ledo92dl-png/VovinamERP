using MediatR;

namespace VovinamERP.Application.Attendance.GetAttendanceRecords;

public sealed record GetAttendanceRecordsQuery(
    Guid TenantId,
    Guid? TrainingSessionId,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<GetAttendanceRecordsResult>;