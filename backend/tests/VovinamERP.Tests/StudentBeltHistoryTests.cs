using VovinamERP.Domain.Students;

namespace VovinamERP.Tests;

public sealed class StudentBeltHistoryTests
{
    [Fact]
    public void Create_PassedResult_WithAwardedDate_ShouldSucceed()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 20),
            "Thi đạt.");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StudentBeltResult.Passed, result.Value.Result);
        Assert.Equal(new DateOnly(2026, 9, 20), result.Value.AwardedDate);
    }

    [Fact]
    public void Create_PassedResult_WithoutAwardedDate_ShouldFail()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            StudentBeltHistoryErrors.AwardedDateRequired,
            result.Error);
    }

    [Fact]
    public void Create_FailedResult_WithoutAwardedDate_ShouldSucceed()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Failed,
            null,
            "Thi chưa đạt.");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StudentBeltResult.Failed, result.Value.Result);
        Assert.Null(result.Value.AwardedDate);
    }

    [Fact]
    public void Create_FailedResult_WithAwardedDate_ShouldFail()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Failed,
            new DateOnly(2026, 9, 20),
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            StudentBeltHistoryErrors.AwardedDateNotAllowed,
            result.Error);
    }

    [Fact]
    public void Create_AwardedDateBeforeExamDate_ShouldFail()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 19),
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            StudentBeltHistoryErrors.AwardedDateBeforeExam,
            result.Error);
    }

    [Fact]
    public void Create_ShouldTrimNote()
    {
        var result = StudentBeltHistory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 9, 20),
            StudentBeltResult.Passed,
            new DateOnly(2026, 9, 20),
            "  Hoàn thành kỳ thi lên đai.  ");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(
            "Hoàn thành kỳ thi lên đai.",
            result.Value.Note);
    }
}
