using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Guardians;

public static class StudentGuardianErrors
{
    public static readonly Error TenantRequired =
        new("STUDENT_GUARDIAN_001", "Tenant is required.");

    public static readonly Error StudentRequired =
        new("STUDENT_GUARDIAN_002", "Student is required.");

    public static readonly Error GuardianRequired =
        new("STUDENT_GUARDIAN_003", "Guardian is required.");

    public static readonly Error AlreadyArchived =
        new("STUDENT_GUARDIAN_004", "Student guardian relationship has already been archived.");
}
