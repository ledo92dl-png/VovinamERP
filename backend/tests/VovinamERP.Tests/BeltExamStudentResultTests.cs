using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Students;
using Xunit;

namespace VovinamERP.Tests;

public sealed class BeltExamStudentResultTests
{
    [Fact]
    public void Create_WithValidData_CreatesResult()
    {
        var tenantId = Guid.NewGuid();
        var beltExamId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = BeltExamStudentResult.Create(
            tenantId,
            beltExamId,
            studentId,
            "  Trấn Biên  ",
            "  Á khoa  ",
            StudentBeltResult.Passed,
            36m,
            2,
            "  Kết quả nhập từ bảng điểm  ",
            userId);

        Assert.True(result.IsSuccess);

        var examResult = result.Value;

        Assert.Equal(tenantId, examResult.TenantId);
        Assert.Equal(beltExamId, examResult.BeltExamId);
        Assert.Equal(studentId, examResult.StudentId);
        Assert.Equal("Trấn Biên", examResult.UnitName);
        Assert.Equal("Á khoa", examResult.SourceResult);
        Assert.Equal(StudentBeltResult.Passed, examResult.Result);
        Assert.Equal(36m, examResult.TotalScore);
        Assert.Equal(2, examResult.Ranking);
        Assert.Equal("Kết quả nhập từ bảng điểm", examResult.Note);
        Assert.Equal(userId, examResult.CreatedBy);
        Assert.False(examResult.IsArchived);
    }

    [Fact]
    public void Create_WithoutTenant_Fails()
    {
        var result = CreateValid(tenantId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.TenantRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutBeltExam_Fails()
    {
        var result = CreateValid(beltExamId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.BeltExamRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutStudent_Fails()
    {
        var result = CreateValid(studentId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.StudentRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutUnitName_Fails()
    {
        var result = CreateValid(unitName: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.UnitNameRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutSourceResult_Fails()
    {
        var result = CreateValid(sourceResult: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.SourceResultRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidResult_Fails()
    {
        var result = CreateValid(
            result: (StudentBeltResult)999);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.InvalidResult.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeTotalScore_Fails()
    {
        var result = CreateValid(totalScore: -0.5m);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.NegativeTotalScore.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithZeroRanking_Fails()
    {
        var result = CreateValid(ranking: 0);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamStudentResultErrors.InvalidRanking.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutOptionalScoreAndRanking_Succeeds()
    {
        var result = BeltExamStudentResult.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Trấn Biên",
            "Đạt",
            StudentBeltResult.Passed,
            null,
            null,
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.TotalScore);
        Assert.Null(result.Value.Ranking);
    }

    private static VovinamERP.SharedKernel.Results.Result<BeltExamStudentResult>
        CreateValid(
            Guid? tenantId = null,
            Guid? beltExamId = null,
            Guid? studentId = null,
            string unitName = "Trấn Biên",
            string sourceResult = "Đạt",
            StudentBeltResult result = StudentBeltResult.Passed,
            decimal totalScore = 36m,
            int ranking = 2)
    {
        return BeltExamStudentResult.Create(
            tenantId ?? Guid.NewGuid(),
            beltExamId ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            unitName,
            sourceResult,
            result,
            totalScore,
            ranking,
            null,
            null);
    }
}