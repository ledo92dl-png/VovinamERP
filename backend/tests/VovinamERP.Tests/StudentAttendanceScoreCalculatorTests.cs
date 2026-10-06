using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Domain.Training;

namespace VovinamERP.Tests;

public class StudentAttendanceScoreCalculatorTests
{
    [Fact]
    public void ExtraTrainingInAnotherWeek_ShouldNotCompensateForMissedSessions()
    {
        var eligibleSessions = new[]
        {
            Eligible(new DateOnly(2026, 10, 5)),
            Eligible(new DateOnly(2026, 10, 7)),
            Eligible(new DateOnly(2026, 10, 9)),
            Eligible(new DateOnly(2026, 10, 12)),
            Eligible(new DateOnly(2026, 10, 13)),
            Eligible(new DateOnly(2026, 10, 14))
        };

        var attendanceSessions = new[]
        {
            Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
            Attendance(new DateOnly(2026, 10, 12), AttendanceStatus.Present),
            Attendance(new DateOnly(2026, 10, 13), AttendanceStatus.Present),
            Attendance(new DateOnly(2026, 10, 14), AttendanceStatus.Present),
            Attendance(new DateOnly(2026, 10, 15), AttendanceStatus.Present, true),
            Attendance(new DateOnly(2026, 10, 16), AttendanceStatus.Present, true)
        };

        var data = new StudentAttendanceScoringData(
            eligibleSessions,
            attendanceSessions);

        var result = StudentAttendanceScoreCalculator.Calculate(
            data,
            AttendanceScorePolicy.Default);

        Assert.Equal(6, result.RequiredSessionCount);
        Assert.Equal(6, result.ActualAttendedSessionCount);
        Assert.Equal(2, result.ExtraAttendanceCount);
        Assert.Equal(2, result.CrossLocationAttendanceCount);
        Assert.Equal(0.6667m, result.AttendanceRate);
        Assert.Equal(66.67m, result.Score);
    }

    [Fact]
    public void NoRequiredSessions_ShouldReturnNoScore()
    {
        var data = new StudentAttendanceScoringData(
            Array.Empty<EligibleAttendanceSessionItem>(),
            Array.Empty<StudentAttendanceSessionItem>());

        var result = StudentAttendanceScoreCalculator.Calculate(
            data,
            AttendanceScorePolicy.Default);

        Assert.Equal(0, result.RequiredSessionCount);
        Assert.Null(result.AttendanceRate);
        Assert.Null(result.Score);
    }

    private static EligibleAttendanceSessionItem Eligible(DateOnly date)
    {
        return new EligibleAttendanceSessionItem(Guid.NewGuid(), date);
    }

    private static StudentAttendanceSessionItem Attendance(
        DateOnly date,
        AttendanceStatus status,
        bool isCrossLocation = false)
    {
        return new StudentAttendanceSessionItem(
            Guid.NewGuid(),
            date,
            status,
            isCrossLocation);
    }
}
