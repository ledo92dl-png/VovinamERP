using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Guardians;

public static class GuardianErrors
{
    public static readonly Error TenantRequired =
        new("GUARDIAN_001", "Tenant is required.");

    public static readonly Error PersonRequired =
        new("GUARDIAN_002", "Person is required.");
}
