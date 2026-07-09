using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Students;

public sealed record CreateStudentRequest(
    Guid TenantId,
    Guid OrganizationId,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    Guid? CurrentBeltRankId,
    DateOnly EnrollmentDate,
    string? MartialName,
    string? IntroducedBy,
    string? MartialProfileNote);
