using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Tests;

public sealed class StudentBeltAgePolicyTests
{
    [Fact]
    public void Validate_Under12_WithJuniorYellowBelt_ShouldSucceed()
    {
        var beltRank = CreateBeltRank(
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            "Hoàng đai thiếu nhi");

        var result = StudentBeltAgePolicy.Validate(
            new DateOnly(2014, 9, 30),
            beltRank,
            new DateOnly(2026, 9, 29));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_Under12_WithYellowBelt_ShouldFail()
    {
        var beltRank = CreateBeltRank(
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai");

        var result = StudentBeltAgePolicy.Validate(
            new DateOnly(2014, 9, 30),
            beltRank,
            new DateOnly(2026, 9, 29));

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_AGE_002",
            result.Error.Code);
    }

    [Fact]
    public void Validate_OnTwelfthBirthday_WithYellowBelt_ShouldSucceed()
    {
        var beltRank = CreateBeltRank(
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai");

        var result = StudentBeltAgePolicy.Validate(
            new DateOnly(2014, 9, 30),
            beltRank,
            new DateOnly(2026, 9, 30));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_OnTwelfthBirthday_WithJuniorYellowBelt_ShouldFail()
    {
        var beltRank = CreateBeltRank(
            StudentBeltAgePolicy.JuniorYellowBeltCode,
            "Hoàng đai thiếu nhi");

        var result = StudentBeltAgePolicy.Validate(
            new DateOnly(2014, 9, 30),
            beltRank,
            new DateOnly(2026, 9, 30));

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_AGE_003",
            result.Error.Code);
    }

    [Fact]
    public void Validate_MissingDateOfBirth_ForAgeSpecificBelt_ShouldFail()
    {
        var beltRank = CreateBeltRank(
            StudentBeltAgePolicy.YellowBeltCode,
            "Hoàng đai");

        var result = StudentBeltAgePolicy.Validate(
            null,
            beltRank,
            new DateOnly(2026, 9, 30));

        Assert.True(result.IsFailure);
        Assert.Equal(
            "STUDENT_BELT_AGE_001",
            result.Error.Code);
    }

    [Fact]
    public void Validate_OtherBelt_WithMissingDateOfBirth_ShouldSucceed()
    {
        var beltRank = CreateBeltRank(
            "LAM-3",
            "Lam đai III");

        var result = StudentBeltAgePolicy.Validate(
            null,
            beltRank,
            new DateOnly(2026, 9, 30));

        Assert.True(result.IsSuccess);
    }

    private static BeltRank CreateBeltRank(
        string code,
        string name)
    {
        var result = BeltRank.Create(
            code,
            name,
            6,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }
}