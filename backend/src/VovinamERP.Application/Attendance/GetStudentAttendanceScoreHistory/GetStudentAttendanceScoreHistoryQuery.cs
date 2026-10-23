using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentAttendanceScoreHistory;

public sealed record GetStudentAttendanceScoreHistoryQuery(
    Guid TenantId,
    Guid StudentId,
    DateOnly FromDate,
    DateOnly ToDate)
    : IRequest<Result<GetStudentAttendanceScoreHistoryResult>>;
