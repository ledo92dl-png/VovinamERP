using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Students;

public sealed class Student : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PersonId { get; private set; }
    public string MemberNumber { get; private set; } = default!;

    public Guid OrganizationId { get; private set; }
    public Guid? CurrentBeltRankId { get; private set; }
    public DateOnly EnrollmentDate { get; private set; }
    public StudentStatus Status { get; private set; }

    public string? MartialName { get; private set; }
    public string? IntroducedBy { get; private set; }
    public string? MartialProfileNote { get; private set; }

    private Student() { }

    private Student(
        Guid tenantId,
        Guid personId,
        string memberNumber,
        Guid organizationId,
        Guid? currentBeltRankId,
        DateOnly enrollmentDate,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        TenantId = tenantId;
        PersonId = personId;
        MemberNumber = memberNumber.Trim();
        OrganizationId = organizationId;
        CurrentBeltRankId = currentBeltRankId;
        EnrollmentDate = enrollmentDate;
        Status = StudentStatus.Active;
        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();

        RaiseDomainEvent(new StudentCreatedEvent(Id, PersonId, MemberNumber));
    }

    public static Result<Student> Create(
        Guid tenantId,
        Guid personId,
        string memberNumber,
        Guid organizationId,
        Guid? currentBeltRankId,
        DateOnly enrollmentDate,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        if (tenantId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.TenantRequired);

        if (personId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.PersonRequired);

        if (string.IsNullOrWhiteSpace(memberNumber))
            return Result<Student>.Failure(StudentErrors.MemberNumberRequired);

        if (organizationId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.OrganizationRequired);

        if (enrollmentDate == default)
            return Result<Student>.Failure(StudentErrors.EnrollmentDateRequired);

        return Result<Student>.Success(new Student(
            tenantId,
            personId,
            memberNumber,
            organizationId,
            currentBeltRankId,
            enrollmentDate,
            martialName,
            introducedBy,
            martialProfileNote));
    }

    public Result UpdateMartialProfile(
        Guid? currentBeltRankId,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote,
        Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        CurrentBeltRankId = currentBeltRankId;
        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();

        MarkUpdated(userId);
        RaiseDomainEvent(new StudentUpdatedEvent(Id));

        return Result.Success();
    }

    public void ChangeStatus(StudentStatus status, Guid? userId)
    {
        Status = status;
        MarkUpdated(userId);
        RaiseDomainEvent(new StudentStatusChangedEvent(Id, status));
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = StudentStatus.Archived;
        base.Archive(userId);
        RaiseDomainEvent(new StudentArchivedEvent(Id));
    }
}
