namespace VovinamERP.Application.Attendance.Scoring;

public static class AttendanceScoreCalculator
{
    public static AttendanceScoreResult Calculate(
        int presentCount,
        int lateCount,
        int excusedCount,
        int absentCount,
        int eligibleSessionCount,
        AttendanceScorePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (presentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(presentCount));

        if (lateCount < 0)
            throw new ArgumentOutOfRangeException(nameof(lateCount));

        if (excusedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(excusedCount));

        if (absentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(absentCount));

        if (eligibleSessionCount < 0)
            throw new ArgumentOutOfRangeException(nameof(eligibleSessionCount));

        if (policy.PresentWeight < 0m || policy.PresentWeight > 1m)
            throw new ArgumentOutOfRangeException(nameof(policy));

        if (policy.LateWeight < 0m || policy.LateWeight > 1m)
            throw new ArgumentOutOfRangeException(nameof(policy));

        if (policy.MaxRequiredSessionsPerWeek <= 0)
            throw new ArgumentOutOfRangeException(nameof(policy));

        var attendedSessionCount = presentCount + lateCount;

        if (eligibleSessionCount == 0)
        {
            return new AttendanceScoreResult(
                presentCount,
                lateCount,
                excusedCount,
                absentCount,
                eligibleSessionCount,
                attendedSessionCount,
                0m,
                0m);
        }

        var attendanceRate = Math.Min(
            (decimal)attendedSessionCount / eligibleSessionCount,
            1m);

        var weightedAttendance =
            presentCount * policy.PresentWeight +
            lateCount * policy.LateWeight;

        var score = Math.Min(
            weightedAttendance / eligibleSessionCount * 100m,
            100m);

        return new AttendanceScoreResult(
            presentCount,
            lateCount,
            excusedCount,
            absentCount,
            eligibleSessionCount,
            attendedSessionCount,
            decimal.Round(
                attendanceRate,
                4,
                MidpointRounding.AwayFromZero),
            decimal.Round(
                score,
                2,
                MidpointRounding.AwayFromZero));
    }
}
