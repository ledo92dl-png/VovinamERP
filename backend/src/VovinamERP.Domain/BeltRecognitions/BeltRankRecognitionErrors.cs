using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltRecognitions;

public static class BeltRankRecognitionErrors
{
    public static readonly Error TenantRequired =
        new("BELT_RECOGNITION_001", "Tenant is required.");

    public static readonly Error StudentRequired =
        new("BELT_RECOGNITION_002", "Student is required.");

    public static readonly Error BeltRankRequired =
        new("BELT_RECOGNITION_003", "Belt rank is required.");

    public static readonly Error RecognitionDateRequired =
        new("BELT_RECOGNITION_004", "Recognition date is required.");

    public static readonly Error InvalidSource =
        new("BELT_RECOGNITION_005", "Recognition source is invalid.");

    public static readonly Error BeltExamStudentResultRequired =
        new(
            "BELT_RECOGNITION_006",
            "Belt exam student result is required for an exam recognition.");

    public static readonly Error BeltExamStudentResultNotAllowed =
        new(
            "BELT_RECOGNITION_007",
            "Belt exam student result is not allowed for an age transition.");
}
