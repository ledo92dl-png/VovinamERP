using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record CreateGuardianRequest(
    Guid TenantId,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    string? RelationshipNote);
