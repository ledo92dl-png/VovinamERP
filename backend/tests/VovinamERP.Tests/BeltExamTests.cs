using VovinamERP.Domain.BeltExams;
using Xunit;

namespace VovinamERP.Tests;

public sealed class BeltExamTests
{
    [Fact]
    public void Create_WithValidData_CreatesBeltExam()
    {
        var tenantId = Guid.NewGuid();
        var targetBeltRankId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var examDate = new DateOnly(2026, 7, 12);

        var result = BeltExam.Create(
            tenantId,
            targetBeltRankId,
            examDate,
            "  Khóa 1  ",
            "  Thành phố Đồng Nai  ",
            "  HOÀNG ĐAI THIẾU NIÊN  ",
            "  Bảng điểm chính thức  ",
            userId);

        Assert.True(result.IsSuccess);

        var exam = result.Value;

        Assert.Equal(tenantId, exam.TenantId);
        Assert.Equal(targetBeltRankId, exam.TargetBeltRankId);
        Assert.Equal(examDate, exam.ExamDate);
        Assert.Equal("Khóa 1", exam.SessionName);
        Assert.Equal("Thành phố Đồng Nai", exam.Location);
        Assert.Equal("HOÀNG ĐAI THIẾU NIÊN", exam.SourceBeltName);
        Assert.Equal("Bảng điểm chính thức", exam.Note);
        Assert.Equal(userId, exam.CreatedBy);
        Assert.False(exam.IsArchived);
    }

    [Fact]
    public void Create_WithoutTenant_Fails()
    {
        var result = CreateValid(
            tenantId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.TenantRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutTargetBeltRank_Fails()
    {
        var result = CreateValid(
            targetBeltRankId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.TargetBeltRankRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutExamDate_Fails()
    {
        var result = BeltExam.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            default,
            "Khóa 1",
            "Thành phố Đồng Nai",
            "HOÀNG ĐAI THIẾU NIÊN",
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.ExamDateRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutSessionName_Fails()
    {
        var result = CreateValid(
            sessionName: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.SessionNameRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutLocation_Fails()
    {
        var result = CreateValid(
            location: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.LocationRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutSourceBeltName_Fails()
    {
        var result = CreateValid(
            sourceBeltName: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamErrors.SourceBeltNameRequired.Code,
            result.Error.Code);
    }

    private static VovinamERP.SharedKernel.Results.Result<BeltExam>
        CreateValid(
            Guid? tenantId = null,
            Guid? targetBeltRankId = null,
            DateOnly? examDate = null,
            string sessionName = "Khóa 1",
            string location = "Thành phố Đồng Nai",
            string sourceBeltName = "HOÀNG ĐAI THIẾU NIÊN")
    {
        return BeltExam.Create(
            tenantId ?? Guid.NewGuid(),
            targetBeltRankId ?? Guid.NewGuid(),
            examDate ?? new DateOnly(2026, 7, 12),
            sessionName,
            location,
            sourceBeltName,
            null,
            null);
    }
}