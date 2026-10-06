using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentAttendanceScore;

public sealed class GetStudentAttendanceScoreQueryHandler
    : IRequestHandler<
        GetStudentAttendanceScoreQuery,
        Result<GetStudentAttendanceScoreResult>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentAttendanceScoreQueryHandler(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GetStudentAttendanceScoreResult>> Handle(
        GetStudentAttendanceScoreQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentAttendanceScoreResult>.Failure(
                new Error(
                    "ATTENDANCE_040",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentAttendanceScoreResult>.Failure(
                new Error(
                    "ATTENDANCE_041",
                    "Student is required."));
        }

        if (request.FromDate == default ||
            request.ToDate == default ||
            request.FromDate > request.ToDate)
        {
            return Result<GetStudentAttendanceScoreResult>.Failure(
                new Error(
                    "ATTENDANCE_042",
                    "Attendance score date range is invalid."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentAttendanceScoreResult>.Failure(
                new Error(
                    "ATTENDANCE_043",
                    "Student was not found."));
        }

        var data =
            await _attendanceRepository.GetStudentAttendanceScoringDataAsync(
                request.TenantId,
                request.StudentId,
                request.FromDate,
                request.ToDate,
                cancellationToken);

        var calculation = StudentAttendanceScoreCalculator.Calculate(
            data,
            AttendanceScorePolicy.Default);

        return Result<GetStudentAttendanceScoreResult>.Success(
            new GetStudentAttendanceScoreResult(
                request.StudentId,
                request.FromDate,
                request.ToDate,
                calculation.RequiredSessionCount,
                calculation.ActualAttendedSessionCount,
                calculation.PresentCount,
                calculation.LateCount,
                calculation.ExcusedCount,
                calculation.AbsentCount,
                calculation.CrossLocationAttendanceCount,
                calculation.ExtraAttendanceCount,
                calculation.AttendanceRate,
                calculation.Score));
    }
}
