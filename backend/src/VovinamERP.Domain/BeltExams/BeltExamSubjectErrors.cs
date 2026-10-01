using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public static class BeltExamSubjectErrors
{
    public static readonly Error TenantRequired =
        new(
            "BELT_EXAM_SUBJECT_001",
            "Tenant is required.");

    public static readonly Error BeltExamRequired =
        new(
            "BELT_EXAM_SUBJECT_002",
            "Belt exam is required.");

    public static readonly Error NameRequired =
        new(
            "BELT_EXAM_SUBJECT_003",
            "Subject name is required.");

    public static readonly Error InvalidDisplayOrder =
        new(
            "BELT_EXAM_SUBJECT_004",
            "Display order must be greater than zero.");

    public static readonly Error InvalidMaximumScore =
        new(
            "BELT_EXAM_SUBJECT_005",
            "Maximum score must be greater than zero when provided.");
}