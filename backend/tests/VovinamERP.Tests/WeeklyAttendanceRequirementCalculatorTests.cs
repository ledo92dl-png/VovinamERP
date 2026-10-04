using VovinamERP.Application.Attendance.Scoring;

namespace VovinamERP.Tests;

public class WeeklyAttendanceRequirementCalculatorTests
{
    [Fact]
    public void TwoAvailableSessions_ShouldRequireTwo()
    {
        var result =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 2 },
                AttendanceScorePolicy.Default);

        Assert.Equal(2, result);
    }

    [Fact]
    public void FiveAvailableSessions_ShouldRequireThree()
    {
        var result =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 5 },
                AttendanceScorePolicy.Default);

        Assert.Equal(3, result);
    }

    [Fact]
    public void MultipleWeeks_ShouldApplyMaximumPerWeekIndependently()
    {
        var result =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 2, 5, 3, 0 },
                AttendanceScorePolicy.Default);

        Assert.Equal(8, result);
    }

    [Fact]
    public void EmptyPeriod_ShouldRequireZero()
    {
        var result =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                Array.Empty<int>(),
                AttendanceScorePolicy.Default);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CustomWeeklyMaximum_ShouldBeSupported()
    {
        var policy = new AttendanceScorePolicy(
            PresentWeight: 1m,
            LateWeight: 1m,
            MaxRequiredSessionsPerWeek: 2);

        var result =
            WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 1, 4, 3 },
                policy);

        Assert.Equal(5, result);
    }

    [Fact]
    public void NegativeWeeklySessionCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 2, -1, 3 },
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
            () => WeeklyAttendanceRequirementCalculator.CalculateRequiredSessions(
                new[] { 2 },
                policy));
    }
}
