using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Instructors;

public static class InstructorErrors
{
    public static readonly Error TenantRequired =
        new("INSTRUCTOR_001", "Tenant is required.");

    public static readonly Error PersonRequired =
        new("INSTRUCTOR_002", "Person is required.");

    public static readonly Error OrganizationRequired =
        new("INSTRUCTOR_003", "Organization is required.");

    public static readonly Error InstructorNumberRequired =
        new("INSTRUCTOR_004", "Instructor number is required.");

    public static readonly Error AlreadyArchived =
        new("INSTRUCTOR_005", "Instructor has already been archived.");
}
