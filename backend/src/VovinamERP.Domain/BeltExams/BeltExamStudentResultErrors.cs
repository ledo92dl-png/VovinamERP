using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public static class BeltExamStudentResultErrors
{
    public static readonly Error TenantRequired =
        new(
            "BELT_EXAM_RESULT_001",
            "Tenant is required.");

    public static readonly Error BeltExamRequired =
        new(
            "BELT_EXAM_RESULT_002",
            "Belt exam is required.");

    public static readonly Error StudentRequired =
        new(
            "BELT_EXAM_RESULT_003",
            "Student is required.");

    public static readonly Error UnitNameRequired =
        new(
            "BELT_EXAM_RESULT_004",
            "Unit name is required.");

    public static readonly Error SourceResultRequired =
        new(
            "BELT_EXAM_RESULT_005",
            "Source result is required.");

    public static readonly Error InvalidResult =
        new(
            "BELT_EXAM_RESULT_006",
            "Student belt result is invalid.");

    public static readonly Error NegativeTotalScore =
        new(
            "BELT_EXAM_RESULT_007",
            "Total score cannot be negative.");

    public static readonly Error InvalidRanking =
        new(
            "BELT_EXAM_RESULT_008",
            "Ranking must be greater than zero when provided.");
}