using VovinamERP.Domain.Guardians;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record UpdateStudentGuardianRequest(
    StudentGuardianRelationship Relationship,
    bool IsPrimary,
    string? Note);
