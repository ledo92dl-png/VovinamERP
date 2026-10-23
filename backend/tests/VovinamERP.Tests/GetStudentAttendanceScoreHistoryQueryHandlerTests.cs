using Moq;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.GetStudentAttendanceScoreHistory;
using VovinamERP.Application.Attendance.Scoring;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Students;
using VovinamERP.Domain.Training;

namespace VovinamERP.Tests;

public class GetStudentAttendanceScoreHistoryQueryHandlerTests
{
    [Fact]
    public async Task History_ShouldIncludeMonthsWithoutData()
    {
        var context = CreateContext(
            eligibleDates:
            [
                new DateOnly(2026, 9, 7),
                new DateOnly(2026, 11, 2)
            ],
            attendance:
            [
                Attendance(
                    new DateOnly(2026, 9, 7),
                    AttendanceStatus.Present),
                Attendance(
                    new DateOnly(2026, 11, 2),
                    AttendanceStatus.Present)
            ]);

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Months.Count);

        Assert.Equal(9, result.Value.Months[0].Month);
        Assert.Equal(100m, result.Value.Months[0].Score);

        Assert.Equal(10, result.Value.Months[1].Month);
        Assert.Equal(0, result.Value.Months[1].RequiredSessionCount);
        Assert.Null(result.Value.Months[1].AttendanceRate);
        Assert.Null(result.Value.Months[1].Score);

        Assert.Equal(11, result.Value.Months[2].Month);
        Assert.Equal(100m, result.Value.Months[2].Score);
    }

    [Fact]
    public async Task History_ShouldCalculateEachMonthIndependently()
    {
        var context = CreateContext(
            eligibleDates:
            [
                new DateOnly(2026, 9, 28),
                new DateOnly(2026, 9, 30),
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 2)
            ],
            attendance:
            [
                Attendance(
                    new DateOnly(2026, 9, 28),
                    AttendanceStatus.Present),
                Attendance(
                    new DateOnly(2026, 10, 1),
                    AttendanceStatus.Present),
                Attendance(
                    new DateOnly(2026, 10, 2),
                    AttendanceStatus.Present)
            ]);

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var september = result.Value.Months.Single(
            x => x.Year == 2026 && x.Month == 9);

        var october = result.Value.Months.Single(
            x => x.Year == 2026 && x.Month == 10);

        Assert.Equal(2, september.RequiredSessionCount);
        Assert.Equal(1, september.ActualAttendedSessionCount);
        Assert.Equal(50m, september.Score);

        Assert.Equal(2, october.RequiredSessionCount);
        Assert.Equal(2, october.ActualAttendedSessionCount);
        Assert.Equal(100m, october.Score);
    }

    [Fact]
    public async Task History_ShouldLoadScoringDataOnlyOnce()
    {
        var context = CreateContext(
            eligibleDates: Array.Empty<DateOnly>(),
            attendance: Array.Empty<StudentAttendanceSessionItem>());

        var result = await context.Handler.Handle(
            context.Query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        context.AttendanceRepository.Verify(
            x => x.GetStudentAttendanceScoringDataAsync(
                context.Query.TenantId,
                context.Query.StudentId,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 11, 30),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
            "MS-HISTORY-001",
            new DateOnly(2026, 1, 1),
            null,
            null);

        if (studentResult.IsFailure)
        {
            throw new InvalidOperationException(
                "Could not create test student.");
        }

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

        var handler =
            new GetStudentAttendanceScoreHistoryQueryHandler(
                attendanceRepository.Object,
                studentRepository.Object);

        var query = new GetStudentAttendanceScoreHistoryQuery(
            tenantId,
            student.Id,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 11, 30));

        return new TestContext(
            handler,
            query,
            attendanceRepository);
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
        GetStudentAttendanceScoreHistoryQueryHandler Handler,
        GetStudentAttendanceScoreHistoryQuery Query,
        Mock<IAttendanceRepository> AttendanceRepository);
}
