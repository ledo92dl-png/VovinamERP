using VovinamERP.Domain.BeltExams;
using Xunit;

namespace VovinamERP.Tests;

public sealed class BeltExamSubjectTests
{
    [Fact]
    public void Create_WithValidData_CreatesSubject()
    {
        var tenantId = Guid.NewGuid();
        var beltExamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = BeltExamSubject.Create(
            tenantId,
            beltExamId,
            "  Lý Thuyết  ",
            1,
            10m,
            userId);

        Assert.True(result.IsSuccess);

        var subject = result.Value;

        Assert.Equal(tenantId, subject.TenantId);
        Assert.Equal(beltExamId, subject.BeltExamId);
        Assert.Equal("Lý Thuyết", subject.Name);
        Assert.Equal(1, subject.DisplayOrder);
        Assert.Equal(10m, subject.MaximumScore);
        Assert.Equal(userId, subject.CreatedBy);
        Assert.False(subject.IsArchived);
    }

    [Fact]
    public void Create_WithoutTenant_Fails()
    {
        var result = CreateValid(tenantId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.TenantRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutBeltExam_Fails()
    {
        var result = CreateValid(beltExamId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.BeltExamRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutName_Fails()
    {
        var result = CreateValid(name: " ");

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.NameRequired.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithZeroDisplayOrder_Fails()
    {
        var result = CreateValid(displayOrder: 0);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.InvalidDisplayOrder.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_Fails()
    {
        var result = CreateValid(displayOrder: -1);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.InvalidDisplayOrder.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithZeroMaximumScore_Fails()
    {
        var result = CreateValid(maximumScore: 0m);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.InvalidMaximumScore.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeMaximumScore_Fails()
    {
        var result = CreateValid(maximumScore: -1m);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltExamSubjectErrors.InvalidMaximumScore.Code,
            result.Error.Code);
    }

    [Fact]
    public void Create_WithoutMaximumScore_Succeeds()
    {
        var result = BeltExamSubject.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Quyền",
            2,
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.MaximumScore);
    }

    private static VovinamERP.SharedKernel.Results.Result<BeltExamSubject>
        CreateValid(
            Guid? tenantId = null,
            Guid? beltExamId = null,
            string name = "Lý Thuyết",
            int displayOrder = 1,
            decimal maximumScore = 10m)
    {
        return BeltExamSubject.Create(
            tenantId ?? Guid.NewGuid(),
            beltExamId ?? Guid.NewGuid(),
            name,
            displayOrder,
            maximumScore,
            null);
    }
}