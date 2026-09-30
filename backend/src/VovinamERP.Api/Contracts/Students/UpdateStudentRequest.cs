using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Students;

public sealed record UpdateStudentRequest(
    Guid TenantId,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    Guid? CurrentBeltRankId,
    string? IntroducedBy,
    string? MartialProfileNote);
