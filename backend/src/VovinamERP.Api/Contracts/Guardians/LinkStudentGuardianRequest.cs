using VovinamERP.Domain.Guardians;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record LinkStudentGuardianRequest(
    Guid TenantId,
    Guid GuardianId,
    StudentGuardianRelationship Relationship,
    bool IsPrimary,
    string? Note);
