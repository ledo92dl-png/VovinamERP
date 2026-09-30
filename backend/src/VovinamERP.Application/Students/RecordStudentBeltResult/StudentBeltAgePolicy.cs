using VovinamERP.Domain.Belts;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RecordStudentBeltResult;

public static class StudentBeltAgePolicy
{
    public const string JuniorYellowBeltCode = "HOANG-TN";
    public const string YellowBeltCode = "HOANG";
    public const int YellowBeltMinimumAge = 12;

    public static Result Validate(
        DateOnly? dateOfBirth,
        BeltRank beltRank,
        DateOnly awardedDate)
    {
        if (beltRank.BeltCode is not JuniorYellowBeltCode
            and not YellowBeltCode)
        {
            return Result.Success();
        }

        if (!dateOfBirth.HasValue)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_AGE_001",
                    "Date of birth is required for yellow belt age validation."));
        }

        var twelfthBirthday =
            dateOfBirth.Value.AddYears(YellowBeltMinimumAge);

        if (beltRank.BeltCode == YellowBeltCode &&
            awardedDate < twelfthBirthday)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_AGE_002",
                    "Students under 12 years old must use the junior yellow belt."));
        }

        if (beltRank.BeltCode == JuniorYellowBeltCode &&
            awardedDate >= twelfthBirthday)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_AGE_003",
                    "Students aged 12 or older must use the yellow belt."));
        }

        return Result.Success();
    }
}