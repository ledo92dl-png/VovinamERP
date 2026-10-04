using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentAttendanceScore;

public sealed record GetStudentAttendanceScoreQuery(
    Guid TenantId,
    Guid StudentId,
    DateOnly FromDate,
    DateOnly ToDate)
    : IRequest<Result<GetStudentAttendanceScoreResult>>;
