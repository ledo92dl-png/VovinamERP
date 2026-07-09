using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.InstructorAssignments;

public static class InstructorAssignmentErrors
{
    public static readonly Error TenantRequired =
        new("INS_ASSIGN_001", "Tenant is required.");

    public static readonly Error TrainingClassRequired =
        new("INS_ASSIGN_002", "Training class is required.");

    public static readonly Error InstructorRequired =
        new("INS_ASSIGN_003", "Instructor is required.");

    public static readonly Error StartDateRequired =
        new("INS_ASSIGN_004", "Start date is required.");

    public static readonly Error InvalidDateRange =
        new("INS_ASSIGN_005", "End date cannot be earlier than start date.");

    public static readonly Error AlreadyArchived =
        new("INS_ASSIGN_006", "Instructor assignment has already been archived.");

    public static readonly Error AlreadyEnded =
        new("INS_ASSIGN_007", "Instructor assignment has already ended.");
}