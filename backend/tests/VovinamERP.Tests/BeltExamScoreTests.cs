using VovinamERP.Domain.BeltExams;
using Xunit;

namespace VovinamERP.Tests;

public sealed class BeltExamScoreTests
{
    [Fact]
    public void Create_WithValidData_CreatesScore()
    {
        var tenantId = Guid.NewGuid();
        var studentResultId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = BeltExamScore.Create(
            tenantId,
            studentResultId,
            subjectId,
            7.5m,
            userId);

        Assert.True(result.IsSuccess);

        var score = result.Value;

        Assert.Equal(tenantId, score.TenantId);
        Assert.Equal(studentResultId, score.BeltExamStudentResultId);
        Assert.Equal(subjectId, score.BeltExamSubjectId);
        Assert.Equal(7.5m, score.Score);
        Assert.Equal(userId, score.CreatedBy);
        Assert.False(score.IsArchived);
    }

    [Fact]
    public void Create_WithoutTenant_Fails()
    {
        var result = BeltExamScore.Create(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            7m,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamScoreErrors.TenantRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutStudentResult_Fails()
    {
        var result = BeltExamScore.Create(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            7m,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamScoreErrors.StudentResultRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutSubject_Fails()
    {
        var result = BeltExamScore.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            7m,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamScoreErrors.SubjectRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeScore_Fails()
    {
        var result = BeltExamScore.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            -0.5m,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamScoreErrors.NegativeScore.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithZeroScore_Succeeds()
    {
        var result = BeltExamScore.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            0m,
            null);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Score);
    }

    [Fact]
    public void Create_WithDecimalScore_PreservesValue()
    {
        var result = BeltExamScore.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            6.25m,
            null);

        Assert.True(result.IsSuccess);
        Assert.Equal(6.25m, result.Value.Score);
    }
}