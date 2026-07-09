using VovinamERP.Domain.Persons;

namespace VovinamERP.Application.Students.RegisterStudent;

public sealed record RegisterStudentCommand(
    Guid TenantId,
    Guid OrganizationId,
    Guid? CurrentBeltRankId,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    DateOnly EnrollmentDate,
    string? MartialName,
    string? IntroducedBy,
    string? MartialProfileNote);
