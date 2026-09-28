using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentMonthlyAttendance;

public sealed class GetStudentMonthlyAttendanceQueryHandler
    : IRequestHandler<
        GetStudentMonthlyAttendanceQuery,
        Result<GetStudentMonthlyAttendanceResult>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentMonthlyAttendanceQueryHandler(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GetStudentMonthlyAttendanceResult>> Handle(
        GetStudentMonthlyAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentMonthlyAttendanceResult>.Failure(
                new Error(
                    "ATTENDANCE_030",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentMonthlyAttendanceResult>.Failure(
                new Error(
                    "ATTENDANCE_031",
                    "Student is required."));
        }

        if (request.Year < 2000)
        {
            return Result<GetStudentMonthlyAttendanceResult>.Failure(
                new Error(
                    "ATTENDANCE_032",
                    "Year is invalid."));
        }

        if (request.Month < 1 || request.Month > 12)
        {
            return Result<GetStudentMonthlyAttendanceResult>.Failure(
                new Error(
                    "ATTENDANCE_033",
                    "Month must be between 1 and 12."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentMonthlyAttendanceResult>.Failure(
                new Error(
                    "ATTENDANCE_034",
                    "Student was not found."));
        }

        var attendanceCount =
            await _attendanceRepository.CountStudentAttendancesByMonthAsync(
                request.TenantId,
                request.StudentId,
                request.Year,
                request.Month,
                cancellationToken);

        return Result<GetStudentMonthlyAttendanceResult>.Success(
            new GetStudentMonthlyAttendanceResult(
                request.StudentId,
                request.Year,
                request.Month,
                attendanceCount));
    }
}