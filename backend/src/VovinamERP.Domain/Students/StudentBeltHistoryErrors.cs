using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Students;

public static class StudentBeltHistoryErrors
{
    public static readonly Error TenantRequired =
        new("STUDENT_BELT_HISTORY_001", "Tenant is required.");

    public static readonly Error StudentRequired =
        new("STUDENT_BELT_HISTORY_002", "Student is required.");

    public static readonly Error BeltRankRequired =
        new("STUDENT_BELT_HISTORY_003", "Belt rank is required.");

    public static readonly Error ExamDateRequired =
        new("STUDENT_BELT_HISTORY_004", "Exam date is required.");

    public static readonly Error InvalidResult =
        new("STUDENT_BELT_HISTORY_005", "Belt result is invalid.");

    public static readonly Error AwardedDateRequired =
        new("STUDENT_BELT_HISTORY_006", "Awarded date is required for a passed belt result.");

    public static readonly Error AwardedDateNotAllowed =
        new("STUDENT_BELT_HISTORY_007", "Awarded date must be empty for a failed belt result.");

    public static readonly Error AwardedDateBeforeExam =
        new("STUDENT_BELT_HISTORY_008", "Awarded date cannot be earlier than exam date.");
}
