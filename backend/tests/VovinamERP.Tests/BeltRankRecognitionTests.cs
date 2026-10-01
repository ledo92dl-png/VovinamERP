using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Tests;

public sealed class BeltRankRecognitionTests
{
    [Fact]
    public void Create_BeltExamRecognition_WithValidData_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var beltRankId = Guid.NewGuid();
        var examResultId = Guid.NewGuid();
        var recognitionDate = new DateOnly(2026, 10, 1);

        var result = BeltRankRecognition.Create(
            tenantId,
            studentId,
            beltRankId,
            recognitionDate,
            BeltRankRecognitionSource.BeltExam,
            examResultId,
            "  Công nhận sau kỳ thi  ");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.Equal(studentId, result.Value.StudentId);
        Assert.Equal(beltRankId, result.Value.BeltRankId);
        Assert.Equal(recognitionDate, result.Value.RecognitionDate);
        Assert.Equal(
            BeltRankRecognitionSource.BeltExam,
            result.Value.Source);
        Assert.Equal(
            examResultId,
            result.Value.BeltExamStudentResultId);
        Assert.Equal(
            "Công nhận sau kỳ thi",
            result.Value.Note);
    }

    [Fact]
    public void Create_AgeTransitionRecognition_WithValidData_ShouldSucceed()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.AgeTransition,
            null,
            "Chuyển Hoàng đai khi đủ 12 tuổi");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(
            BeltRankRecognitionSource.AgeTransition,
            result.Value.Source);
        Assert.Null(result.Value.BeltExamStudentResultId);
    }

    [Fact]
    public void Create_EmptyTenant_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.AgeTransition,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_001",
            result.Error.Code);
    }

    [Fact]
    public void Create_EmptyStudent_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.AgeTransition,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_002",
            result.Error.Code);
    }

    [Fact]
    public void Create_EmptyBeltRank_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.AgeTransition,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_003",
            result.Error.Code);
    }

    [Fact]
    public void Create_DefaultRecognitionDate_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            default,
            BeltRankRecognitionSource.AgeTransition,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_004",
            result.Error.Code);
    }

    [Fact]
    public void Create_InvalidSource_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            (BeltRankRecognitionSource)999,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_005",
            result.Error.Code);
    }

    [Fact]
    public void Create_BeltExamRecognition_WithoutExamResult_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.BeltExam,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_006",
            result.Error.Code);
    }

    [Fact]
    public void Create_AgeTransitionRecognition_WithExamResult_ShouldFail()
    {
        var result = BeltRankRecognition.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            BeltRankRecognitionSource.AgeTransition,
            Guid.NewGuid(),
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RECOGNITION_007",
            result.Error.Code);
    }
}
