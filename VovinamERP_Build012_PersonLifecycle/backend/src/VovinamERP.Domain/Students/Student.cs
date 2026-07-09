using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Students;

public sealed class Student : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PersonId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid? CurrentBeltRankId { get; private set; }

    public string MemberNumber { get; private set; } = default!;
    public DateOnly EnrollmentDate { get; private set; }
    public StudentStatus Status { get; private set; }

    public string? MartialName { get; private set; }
    public string? IntroducedBy { get; private set; }
    public string? MartialProfileNote { get; private set; }

    private Student() { }

    private Student(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        string memberNumber,
        DateOnly enrollmentDate,
        Guid? currentBeltRankId,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        TenantId = tenantId;
        PersonId = personId;
        OrganizationId = organizationId;
        MemberNumber = memberNumber.Trim();
        EnrollmentDate = enrollmentDate;
        CurrentBeltRankId = currentBeltRankId;
        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();
        Status = StudentStatus.Active;

        RaiseDomainEvent(new StudentCreatedEvent(Id, TenantId, PersonId, MemberNumber));
    }

    public static Result<Student> Create(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        string memberNumber,
        DateOnly enrollmentDate,
        Guid? currentBeltRankId,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        if (tenantId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.TenantRequired);

        if (personId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.PersonRequired);

        if (organizationId == Guid.Empty)
            return Result<Student>.Failure(StudentErrors.OrganizationRequired);

        if (string.IsNullOrWhiteSpace(memberNumber))
            return Result<Student>.Failure(StudentErrors.MemberNumberRequired);

        if (enrollmentDate == default)
            return Result<Student>.Failure(StudentErrors.EnrollmentDateRequired);

        return Result<Student>.Success(
            new Student(
                tenantId,
                personId,
                organizationId,
                memberNumber,
                enrollmentDate,
                currentBeltRankId,
                martialName,
                introducedBy,
                martialProfileNote));
    }

    public Result UpdateMartialProfile(
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();

        MarkUpdated(null);
        RaiseDomainEvent(new StudentMartialProfileUpdatedEvent(Id));

        return Result.Success();
    }

    public Result ChangeStatus(StudentStatus status)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        Status = status;
        MarkUpdated(null);
        RaiseDomainEvent(new StudentStatusChangedEvent(Id, Status));

        return Result.Success();
    }

    public Result ChangeBelt(Guid beltRankId)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        if (beltRankId == Guid.Empty)
            return Result.Failure(new Error("STUDENT_007", "Belt rank is required."));

        CurrentBeltRankId = beltRankId;
        MarkUpdated(null);
        RaiseDomainEvent(new StudentBeltChangedEvent(Id, beltRankId));

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = StudentStatus.Archived;
        base.Archive(userId);
        RaiseDomainEvent(new StudentArchivedEvent(Id));
    }
}
