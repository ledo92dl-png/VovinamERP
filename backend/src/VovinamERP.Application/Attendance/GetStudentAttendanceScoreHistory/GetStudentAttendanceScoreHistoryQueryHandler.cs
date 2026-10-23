using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.GetStudentAttendanceScoreHistory;

public sealed class GetStudentAttendanceScoreHistoryQueryHandler
    : IRequestHandler<
        GetStudentAttendanceScoreHistoryQuery,
        Result<GetStudentAttendanceScoreHistoryResult>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentAttendanceScoreHistoryQueryHandler(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GetStudentAttendanceScoreHistoryResult>> Handle(
        GetStudentAttendanceScoreHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentAttendanceScoreHistoryResult>.Failure(
                new Error("ATTENDANCE_040", "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentAttendanceScoreHistoryResult>.Failure(
                new Error("ATTENDANCE_041", "Student is required."));
        }

        if (request.FromDate == default ||
            request.ToDate == default ||
            request.FromDate > request.ToDate)
        {
            return Result<GetStudentAttendanceScoreHistoryResult>.Failure(
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
            return Result<GetStudentAttendanceScoreHistoryResult>.Failure(
                new Error("ATTENDANCE_043", "Student was not found."));
        }

        var data =
            await _attendanceRepository.GetStudentAttendanceScoringDataAsync(
                request.TenantId,
                request.StudentId,
                request.FromDate,
                request.ToDate,
                cancellationToken);

        var months = new List<StudentAttendanceScoreMonthResult>();

        var monthCursor = new DateOnly(
            request.FromDate.Year,
            request.FromDate.Month,
            1);

        var lastMonth = new DateOnly(
            request.ToDate.Year,
            request.ToDate.Month,
            1);

        while (monthCursor <= lastMonth)
        {
            var calendarMonthEnd = monthCursor.AddMonths(1).AddDays(-1);

            var monthFromDate =
                request.FromDate > monthCursor
                    ? request.FromDate
                    : monthCursor;

            var monthToDate =
                request.ToDate < calendarMonthEnd
                    ? request.ToDate
                    : calendarMonthEnd;

            var monthData = new StudentAttendanceScoringData(
                data.EligibleSessions
                    .Where(x =>
                        x.SessionDate >= monthFromDate &&
                        x.SessionDate <= monthToDate)
                    .ToList(),
                data.AttendanceSessions
                    .Where(x =>
                        x.SessionDate >= monthFromDate &&
                        x.SessionDate <= monthToDate)
                    .ToList());

            var calculation =
                StudentAttendanceScoreCalculator.Calculate(
                    monthData,
                    AttendanceScorePolicy.Default);

            months.Add(
                new StudentAttendanceScoreMonthResult(
                    monthCursor.Year,
                    monthCursor.Month,
                    monthFromDate,
                    monthToDate,
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

            monthCursor = monthCursor.AddMonths(1);
        }

        return Result<GetStudentAttendanceScoreHistoryResult>.Success(
            new GetStudentAttendanceScoreHistoryResult(
                request.StudentId,
                request.FromDate,
                request.ToDate,
                months));
    }
}
