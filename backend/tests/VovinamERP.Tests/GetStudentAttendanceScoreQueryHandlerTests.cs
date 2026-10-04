using Moq;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.GetStudentAttendanceScore;
using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Students;
using VovinamERP.Domain.Training;

namespace VovinamERP.Tests;

public class GetStudentAttendanceScoreQueryHandlerTests
{
    [Fact]
    public async Task TwoAvailableSessions_ShouldRequireTwo()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 7), AttendanceStatus.Present)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.RequiredSessionCount);
        Assert.Equal(2, result.Value.ActualAttendedSessionCount);
        Assert.Equal(100m, result.Value.Score);
    }

    [Fact]
    public async Task FiveAvailableSessionsInOneWeek_ShouldRequireThree()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 6),
                new DateOnly(2026, 10, 7),
                new DateOnly(2026, 10, 8),
                new DateOnly(2026, 10, 9)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 6), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 7), AttendanceStatus.Present)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.RequiredSessionCount);
        Assert.Equal(3, result.Value.ActualAttendedSessionCount);
        Assert.Equal(100m, result.Value.Score);
    }

    [Fact]
    public async Task WeeklyMaximum_ShouldApplyIndependentlyAcrossWeeks()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7),

                new DateOnly(2026, 10, 12),
                new DateOnly(2026, 10, 13),
                new DateOnly(2026, 10, 14),
                new DateOnly(2026, 10, 15),
                new DateOnly(2026, 10, 16)
            },
            attendance: Array.Empty<StudentAttendanceSessionItem>());

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.RequiredSessionCount);
        Assert.Equal(0m, result.Value.Score);
    }

    [Fact]
    public async Task MondayThroughSunday_ShouldBeOneWeek()
    {
        var dates = Enumerable
            .Range(0, 7)
            .Select(offset => new DateOnly(2026, 10, 5).AddDays(offset))
            .ToArray();

        var context = CreateContext(
            eligibleDates: dates,
            attendance: Array.Empty<StudentAttendanceSessionItem>());

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.RequiredSessionCount);
    }

    [Fact]
    public async Task ExtraTraining_ShouldRemainOneHundredAndBeReported()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 7), AttendanceStatus.Present),
                Attendance(
                    new DateOnly(2026, 10, 9),
                    AttendanceStatus.Present,
                    isCrossLocation: true)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.RequiredSessionCount);
        Assert.Equal(3, result.Value.ActualAttendedSessionCount);
        Assert.Equal(1, result.Value.ExtraAttendanceCount);
        Assert.Equal(1, result.Value.CrossLocationAttendanceCount);
        Assert.Equal(100m, result.Value.Score);
    }

    [Fact]
    public async Task ExtraTrainingInAnotherWeek_ShouldNotCompensateForMissedSessions()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7),
                new DateOnly(2026, 10, 9),
                new DateOnly(2026, 10, 12),
                new DateOnly(2026, 10, 13),
                new DateOnly(2026, 10, 14)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 12), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 13), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 14), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 15), AttendanceStatus.Present, true),
                Attendance(new DateOnly(2026, 10, 16), AttendanceStatus.Present, true)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value.RequiredSessionCount);
        Assert.Equal(6, result.Value.ActualAttendedSessionCount);
        Assert.Equal(2, result.Value.ExtraAttendanceCount);
        Assert.Equal(2, result.Value.CrossLocationAttendanceCount);
        Assert.Equal(0.6667m, result.Value.AttendanceRate);
        Assert.Equal(66.67m, result.Value.Score);
    }
    [Fact]
    public async Task PresentAndLate_ShouldCountAsAttended()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 7), AttendanceStatus.Late)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PresentCount);
        Assert.Equal(1, result.Value.LateCount);
        Assert.Equal(2, result.Value.ActualAttendedSessionCount);
        Assert.Equal(100m, result.Value.Score);
    }

    [Fact]
    public async Task ExcusedAndAbsent_ShouldNotCountAsAttended()
    {
        var context = CreateContext(
            eligibleDates: new[]
            {
                new DateOnly(2026, 10, 5),
                new DateOnly(2026, 10, 7),
                new DateOnly(2026, 10, 9)
            },
            attendance: new[]
            {
                Attendance(new DateOnly(2026, 10, 5), AttendanceStatus.Present),
                Attendance(new DateOnly(2026, 10, 7), AttendanceStatus.Excused),
                Attendance(new DateOnly(2026, 10, 9), AttendanceStatus.Absent)
            });

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ActualAttendedSessionCount);
        Assert.Equal(1, result.Value.ExcusedCount);
        Assert.Equal(1, result.Value.AbsentCount);
        Assert.Equal(33.33m, result.Value.Score);
    }

    [Fact]
    public async Task NoRequiredSessions_ShouldReturnNoScore()
    {
        var context = CreateContext(
            eligibleDates: Array.Empty<DateOnly>(),
            attendance: Array.Empty<StudentAttendanceSessionItem>());

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.RequiredSessionCount);
        Assert.Null(result.Value.AttendanceRate);
        Assert.Null(result.Value.Score);
    }

    private static TestContext CreateContext(
        IReadOnlyList<DateOnly> eligibleDates,
        IReadOnlyList<StudentAttendanceSessionItem> attendance)
    {
        var tenantId = Guid.NewGuid();

        var studentResult = Student.Register(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "MS-TEST-001",
            new DateOnly(2026, 1, 1),
            null,
            null);

        if (studentResult.IsFailure)
            throw new InvalidOperationException(
                "Could not create test student.");

        var student = studentResult.Value;

        var eligibleSessions = eligibleDates
            .Select(date => new EligibleAttendanceSessionItem(
                Guid.NewGuid(),
                date))
            .ToList();

        var data = new StudentAttendanceScoringData(
            eligibleSessions,
            attendance);

        var attendanceRepository =
            new Mock<IAttendanceRepository>();

        attendanceRepository
            .Setup(x => x.GetStudentAttendanceScoringDataAsync(
                tenantId,
                student.Id,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var studentRepository =
            new Mock<IStudentRepository>();

        studentRepository
            .Setup(x => x.GetByIdAsync(
                tenantId,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        var handler = new GetStudentAttendanceScoreQueryHandler(
            attendanceRepository.Object,
            studentRepository.Object);

        var query = new GetStudentAttendanceScoreQuery(
            tenantId,
            student.Id,
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 31));

        return new TestContext(handler, query);
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

    private sealed record TestContext(
        GetStudentAttendanceScoreQueryHandler Handler,
        GetStudentAttendanceScoreQuery Query);
}
