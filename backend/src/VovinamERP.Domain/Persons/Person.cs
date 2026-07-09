using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Persons;

public sealed class Person : AggregateRoot
{
    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public Gender Gender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }

    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? AvatarUrl { get; private set; }

    private Person() { }

    private Person(
        Guid tenantId,
        string code,
        string fullName,
        Gender gender,
        DateOnly? dateOfBirth,
        string? phoneNumber,
        string? email,
        string? address,
        string? avatarUrl)
    {
        TenantId = tenantId;
        Code = code.Trim();
        FullName = fullName.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
        AvatarUrl = avatarUrl?.Trim();

        RaiseDomainEvent(new PersonCreatedEvent(Id, TenantId, Code, FullName));
    }

    public static Result<Person> Create(
        Guid tenantId,
        string code,
        string fullName,
        Gender gender,
        DateOnly? dateOfBirth,
        string? phoneNumber,
        string? email,
        string? address,
        string? avatarUrl)
    {
        if (tenantId == Guid.Empty)
            return Result<Person>.Failure(PersonErrors.TenantRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result<Person>.Failure(PersonErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(fullName))
            return Result<Person>.Failure(PersonErrors.FullNameRequired);

        return Result<Person>.Success(
            new Person(tenantId, code, fullName, gender, dateOfBirth, phoneNumber, email, address, avatarUrl));
    }

    public Result UpdateBasicInfo(
        string fullName,
        Gender gender,
        DateOnly? dateOfBirth,
        string? phoneNumber,
        string? email,
        string? address,
        string? avatarUrl)
    {
        if (IsArchived)
            return Result.Failure(PersonErrors.AlreadyArchived);

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(PersonErrors.FullNameRequired);

        FullName = fullName.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
        AvatarUrl = avatarUrl?.Trim();

        MarkUpdated(null);
        RaiseDomainEvent(new PersonUpdatedEvent(Id));

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        base.Archive(userId);
        RaiseDomainEvent(new PersonArchivedEvent(Id));
    }
}
