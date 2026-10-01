using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public static class BeltExamErrors
{
    public static readonly Error TenantRequired =
        new(
            "BELT_EXAM_001",
            "Tenant is required.");

    public static readonly Error TargetBeltRankRequired =
        new(
            "BELT_EXAM_002",
            "Target belt rank is required.");

    public static readonly Error ExamDateRequired =
        new(
            "BELT_EXAM_003",
            "Exam date is required.");

    public static readonly Error SessionNameRequired =
        new(
            "BELT_EXAM_004",
            "Exam session name is required.");

    public static readonly Error LocationRequired =
        new(
            "BELT_EXAM_005",
            "Exam location is required.");

    public static readonly Error SourceBeltNameRequired =
        new(
            "BELT_EXAM_006",
            "Source belt name is required.");
}