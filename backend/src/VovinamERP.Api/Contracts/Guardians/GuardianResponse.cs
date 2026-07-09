using VovinamERP.Domain.Guardians;
using VovinamERP.Domain.Persons;

namespace VovinamERP.Api.Contracts.Guardians;

public sealed record GuardianResponse(
    Guid GuardianId,
    Guid PersonId,
    Guid TenantId,
    string PersonCode,
    string FullName,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? Email,
    string? Address,
    string? AvatarUrl,
    string? RelationshipNote)
{
    public static GuardianResponse FromDomain(Guardian guardian, Person person)
    {
        return new GuardianResponse(
            guardian.Id,
            person.Id,
            guardian.TenantId,
            person.Code,
            person.FullName,
            person.Gender,
            person.DateOfBirth,
            person.PhoneNumber,
            person.Email,
            person.Address,
            person.AvatarUrl,
            guardian.RelationshipNote);
    }
}
