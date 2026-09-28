using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentMonthlyAttendance;

public sealed record GetStudentMonthlyAttendanceQuery(
    Guid TenantId,
    Guid StudentId,
    int Year,
    int Month)
    : IRequest<Result<GetStudentMonthlyAttendanceResult>>;