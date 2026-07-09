using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Persons;

public static class PersonErrors
{
    public static readonly Error TenantRequired =
        new("PERSON_001", "Tenant is required.");

    public static readonly Error CodeRequired =
        new("PERSON_002", "Person code is required.");

    public static readonly Error FullNameRequired =
        new("PERSON_003", "Full name is required.");

    public static readonly Error AlreadyArchived =
        new("PERSON_004", "Person has already been archived.");
}
