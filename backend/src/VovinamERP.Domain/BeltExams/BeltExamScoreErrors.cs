using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public static class BeltExamScoreErrors
{
    public static readonly Error TenantRequired =
        new(
            "BELT_EXAM_SCORE_001",
            "Tenant is required.");

    public static readonly Error StudentResultRequired =
        new(
            "BELT_EXAM_SCORE_002",
            "Belt exam student result is required.");

    public static readonly Error SubjectRequired =
        new(
            "BELT_EXAM_SCORE_003",
            "Belt exam subject is required.");

    public static readonly Error NegativeScore =
        new(
            "BELT_EXAM_SCORE_004",
            "Score cannot be negative.");
}