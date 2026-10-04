namespace VovinamERP.Application.Attendance.Scoring;

public static class WeeklyAttendanceRequirementCalculator
{
    public static int CalculateRequiredSessions(
        IEnumerable<int> eligibleSessionsByWeek,
        AttendanceScorePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(eligibleSessionsByWeek);
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.MaxRequiredSessionsPerWeek <= 0)
            throw new ArgumentOutOfRangeException(nameof(policy));

        var requiredSessions = 0;

        foreach (var eligibleSessions in eligibleSessionsByWeek)
        {
            if (eligibleSessions < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(eligibleSessionsByWeek));

            requiredSessions += Math.Min(
                eligibleSessions,
                policy.MaxRequiredSessionsPerWeek);
        }

        return requiredSessions;
    }
}
