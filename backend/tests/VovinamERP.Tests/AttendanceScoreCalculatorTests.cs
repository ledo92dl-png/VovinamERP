using VovinamERP.Application.Attendance.Scoring;

namespace VovinamERP.Tests;

public class AttendanceScoreCalculatorTests
{
    [Fact]
    public void FullAttendance_ShouldScoreOneHundred()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 10,
            lateCount: 2,
            excusedCount: 0,
            absentCount: 0,
            eligibleSessionCount: 12,
            AttendanceScorePolicy.Default);

        Assert.Equal(12, result.AttendedSessionCount);
        Assert.Equal(1m, result.AttendanceRate);
        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void ExcusedAndAbsent_ShouldBothReduceAttendanceScore()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 7,
            lateCount: 1,
            excusedCount: 1,
            absentCount: 1,
            eligibleSessionCount: 10,
            AttendanceScorePolicy.Default);

        Assert.Equal(8, result.AttendedSessionCount);
        Assert.Equal(0.8m, result.AttendanceRate);
        Assert.Equal(80m, result.Score);
    }

    [Fact]
    public void ElevenOfTwelve_ShouldCalculateExactPercentage()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 10,
            lateCount: 1,
            excusedCount: 0,
            absentCount: 1,
            eligibleSessionCount: 12,
            AttendanceScorePolicy.Default);

        Assert.Equal(11, result.AttendedSessionCount);
        Assert.Equal(0.9167m, result.AttendanceRate);
        Assert.Equal(91.67m, result.Score);
    }

    [Fact]
    public void NoEligibleSessions_ShouldReturnZeroScore()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 0,
            lateCount: 0,
            excusedCount: 0,
            absentCount: 0,
            eligibleSessionCount: 0,
            AttendanceScorePolicy.Default);

        Assert.Equal(0m, result.AttendanceRate);
        Assert.Equal(0m, result.Score);
    }

    [Fact]
    public void CustomLateWeight_ShouldBeSupported()
    {
        var policy = new AttendanceScorePolicy(
            PresentWeight: 1m,
            LateWeight: 0.5m,
            MaxRequiredSessionsPerWeek: 3);

        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 8,
            lateCount: 2,
            excusedCount: 0,
            absentCount: 0,
            eligibleSessionCount: 10,
            policy);

        Assert.Equal(10, result.AttendedSessionCount);
        Assert.Equal(1m, result.AttendanceRate);
        Assert.Equal(90m, result.Score);
    }

    [Fact]
    public void ExtraTraining_ShouldRemainOneHundred()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 3,
            lateCount: 0,
            excusedCount: 0,
            absentCount: 0,
            eligibleSessionCount: 2,
            AttendanceScorePolicy.Default);

        Assert.Equal(3, result.AttendedSessionCount);
        Assert.Equal(1m, result.AttendanceRate);
        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void ExtraTraining_ShouldStillBeReported()
    {
        var result = AttendanceScoreCalculator.Calculate(
            presentCount: 5,
            lateCount: 0,
            excusedCount: 0,
            absentCount: 0,
            eligibleSessionCount: 3,
            AttendanceScorePolicy.Default);

        Assert.Equal(5, result.AttendedSessionCount);
        Assert.Equal(3, result.EligibleSessionCount);
        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void NegativeCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AttendanceScoreCalculator.Calculate(
                presentCount: -1,
                lateCount: 0,
                excusedCount: 0,
                absentCount: 0,
                eligibleSessionCount: 10,
                AttendanceScorePolicy.Default));
    }

    [Fact]
    public void InvalidWeeklyMaximum_ShouldThrow()
    {
        var policy = new AttendanceScorePolicy(
            PresentWeight: 1m,
            LateWeight: 1m,
            MaxRequiredSessionsPerWeek: 0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => AttendanceScoreCalculator.Calculate(
                presentCount: 1,
                lateCount: 0,
                excusedCount: 0,
                absentCount: 0,
                eligibleSessionCount: 1,
                policy));
    }
}
