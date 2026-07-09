using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record UpdateGuardianRequest(
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    string? RelationshipNote);
