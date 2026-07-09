using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;

namespace VovinamERP.Api.Contracts.Students;

public sealed record StudentResponse(
    Guid StudentId,
    Guid PersonId,
    Guid TenantId,
    Guid OrganizationId,
    string MemberNumber,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    Guid? CurrentBeltRankId,
    DateOnly EnrollmentDate,
    StudentStatus Status,
    string? MartialName,
    string? IntroducedBy,
    string? MartialProfileNote);
