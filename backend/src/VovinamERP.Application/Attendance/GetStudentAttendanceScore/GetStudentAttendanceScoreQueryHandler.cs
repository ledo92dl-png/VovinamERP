using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Training;
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

        var policy = AttendanceScorePolicy.Default;

        var eligibleSessionsByWeek = data.EligibleSessions
            .GroupBy(x => GetWeekStart(x.SessionDate))
            .Select(group => group.Count());

        var requiredSessionCount =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                eligibleSessionsByWeek,
                policy);

        var presentCount = data.AttendanceSessions.Count(
            x => x.Status == AttendanceStatus.Present);

        var lateCount = data.AttendanceSessions.Count(
            x => x.Status == AttendanceStatus.Late);

        var excusedCount = data.AttendanceSessions.Count(
            x => x.Status == AttendanceStatus.Excused);

        var absentCount = data.AttendanceSessions.Count(
            x => x.Status == AttendanceStatus.Absent);

        var actualAttendedSessionCount =
            presentCount + lateCount;

        var crossLocationAttendanceCount =
            data.AttendanceSessions.Count(
                x => x.IsCrossLocation &&
                     (x.Status == AttendanceStatus.Present ||
                      x.Status == AttendanceStatus.Late));

        var attendanceByWeek = data.AttendanceSessions
            .Where(x =>
                x.Status == AttendanceStatus.Present ||
                x.Status == AttendanceStatus.Late)
            .GroupBy(x => GetWeekStart(x.SessionDate))
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        var eligibleByWeek = data.EligibleSessions
            .GroupBy(x => GetWeekStart(x.SessionDate))
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        var creditedAttendedSessionCount = 0;
        var extraAttendanceCount = 0;
        decimal creditedWeightedAttendance = 0m;

        foreach (var week in attendanceByWeek.Keys.Union(eligibleByWeek.Keys))
        {
            var eligibleInWeek = eligibleByWeek.GetValueOrDefault(week, 0);

            var requiredInWeek = Math.Min(
                eligibleInWeek,
                policy.MaxRequiredSessionsPerWeek);

            attendanceByWeek.TryGetValue(
                week,
                out var attendanceInWeek);

            attendanceInWeek ??= [];

            var presentInWeek = attendanceInWeek.Count(
                x => x.Status == AttendanceStatus.Present);

            var lateInWeek = attendanceInWeek.Count(
                x => x.Status == AttendanceStatus.Late);

            var attendedInWeek = presentInWeek + lateInWeek;

            creditedAttendedSessionCount += Math.Min(
                attendedInWeek,
                requiredInWeek);

            extraAttendanceCount += Math.Max(
                attendedInWeek - requiredInWeek,
                0);

            var weightedAttendanceInWeek =
                (presentInWeek * policy.PresentWeight) +
                (lateInWeek * policy.LateWeight);

            creditedWeightedAttendance += Math.Min(
                weightedAttendanceInWeek,
                requiredInWeek);
        }

        decimal? attendanceRate = null;
        decimal? score = null;

        if (requiredSessionCount > 0)
        {
            attendanceRate = Math.Round(
                Math.Min(
                    (decimal)creditedAttendedSessionCount /
                    requiredSessionCount,
                    1m),
                4);

            score = Math.Round(
                Math.Min(
                    creditedWeightedAttendance /
                    requiredSessionCount *
                    100m,
                    100m),
                2);
        }

        return Result<GetStudentAttendanceScoreResult>.Success(
            new GetStudentAttendanceScoreResult(
                request.StudentId,
                request.FromDate,
                request.ToDate,
                requiredSessionCount,
                actualAttendedSessionCount,
                presentCount,
                lateCount,
                excusedCount,
                absentCount,
                crossLocationAttendanceCount,
                extraAttendanceCount,
                attendanceRate,
                score));
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var daysSinceMonday =
            ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return date.AddDays(-daysSinceMonday);
    }
}
