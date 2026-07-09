using VovinamERP.Domain.Guardians;
using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record StudentGuardianResponse(
    Guid Id,
    Guid StudentId,
    Guid GuardianId,
    Guid PersonId,
    Guid TenantId,
    string GuardianCode,
    string GuardianFullName,
    Gender GuardianGender,
    string? GuardianPhoneNumber,
    string? GuardianEmail,
    StudentGuardianRelationship Relationship,
    bool IsPrimary,
    string? Note)
{
    public static StudentGuardianResponse FromDomain(
        StudentGuardian link,
        Guardian guardian,
        Person person)
    {
        return new StudentGuardianResponse(
            link.Id,
            link.StudentId,
            link.GuardianId,
            guardian.PersonId,
            link.TenantId,
            person.Code,
            person.FullName,
            person.Gender,
            person.PhoneNumber,
            person.Email,
            link.Relationship,
            link.IsPrimary,
            link.Note);
    }
}
