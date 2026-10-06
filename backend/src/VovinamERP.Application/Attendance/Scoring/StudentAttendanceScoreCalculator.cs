using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.Scoring;

public static class StudentAttendanceScoreCalculator
{
    public static StudentAttendanceScoreCalculationResult Calculate(
        StudentAttendanceScoringData data,
        AttendanceScorePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.PresentWeight < 0m || policy.PresentWeight > 1m)
            throw new ArgumentOutOfRangeException(nameof(policy));

        if (policy.LateWeight < 0m || policy.LateWeight > 1m)
            throw new ArgumentOutOfRangeException(nameof(policy));

        if (policy.MaxRequiredSessionsPerWeek <= 0)
            throw new ArgumentOutOfRangeException(nameof(policy));

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

        var actualAttendedSessionCount = presentCount + lateCount;

        var crossLocationAttendanceCount = data.AttendanceSessions.Count(
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
                    (decimal)creditedAttendedSessionCount / requiredSessionCount,
                    1m),
                4);

            score = Math.Round(
                Math.Min(
                    creditedWeightedAttendance / requiredSessionCount * 100m,
                    100m),
                2);
        }

        return new StudentAttendanceScoreCalculationResult(
            requiredSessionCount,
            actualAttendedSessionCount,
            presentCount,
            lateCount,
            excusedCount,
            absentCount,
            crossLocationAttendanceCount,
            extraAttendanceCount,
            attendanceRate,
            score);
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var daysSinceMonday =
            ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return date.AddDays(-daysSinceMonday);
    }
}
