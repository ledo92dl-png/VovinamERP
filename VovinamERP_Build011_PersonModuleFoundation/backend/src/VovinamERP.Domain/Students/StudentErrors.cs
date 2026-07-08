using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Students;

public static class StudentErrors
{
    public static readonly Error TenantRequired =
        new("STUDENT_001", "Tenant is required.");

    public static readonly Error PersonRequired =
        new("STUDENT_002", "Person is required.");

    public static readonly Error MemberNumberRequired =
        new("STUDENT_003", "Member number is required.");

    public static readonly Error OrganizationRequired =
        new("STUDENT_004", "Organization is required.");

    public static readonly Error EnrollmentDateRequired =
        new("STUDENT_005", "Enrollment date is required.");

    public static readonly Error AlreadyArchived =
        new("STUDENT_006", "Student has already been archived.");
}
