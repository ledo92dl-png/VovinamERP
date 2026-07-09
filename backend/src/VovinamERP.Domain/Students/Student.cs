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

    private Student()
    {
    }

    private Student(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        Guid? currentBeltRankId,
        string memberNumber,
        DateOnly enrollmentDate,
        string? martialName,
        string? introducedBy,
        string? martialProfileNote)
    {
        TenantId = tenantId;
        PersonId = personId;
        OrganizationId = organizationId;
        CurrentBeltRankId = currentBeltRankId;
        MemberNumber = memberNumber.Trim();
        EnrollmentDate = enrollmentDate;
        Status = StudentStatus.Active;
        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();

        RaiseDomainEvent(new StudentRegisteredEvent(Id, TenantId, PersonId, MemberNumber));
    }

    public static Result<Student> Register(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        Guid? currentBeltRankId,
        string memberNumber,
        DateOnly enrollmentDate,
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

        var student = new Student(
            tenantId,
            personId,
            organizationId,
            currentBeltRankId,
            memberNumber,
            enrollmentDate,
            martialName,
            introducedBy,
            martialProfileNote);

        return Result<Student>.Success(student);
    }

    public Result UpdateMartialProfile(
        string? martialName,
        string? introducedBy,
        string? martialProfileNote,
        Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        MartialName = martialName?.Trim();
        IntroducedBy = introducedBy?.Trim();
        MartialProfileNote = martialProfileNote?.Trim();

        MarkUpdated(userId);
        RaiseDomainEvent(new StudentMartialProfileUpdatedEvent(Id));

        return Result.Success();
    }

    public Result ChangeStatus(StudentStatus newStatus, string? reason, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        if (Status == newStatus)
            return Result.Success();

        var oldStatus = Status;
        Status = newStatus;

        MarkUpdated(userId);
        RaiseDomainEvent(new StudentStatusChangedEvent(Id, oldStatus, newStatus, reason));

        return Result.Success();
    }

    public Result ChangeCurrentBelt(Guid beltRankId, DateOnly awardedDate, string? note, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentErrors.AlreadyArchived);

        if (beltRankId == Guid.Empty)
            return Result.Failure(StudentErrors.BeltRankRequired);

        CurrentBeltRankId = beltRankId;

        MarkUpdated(userId);
        RaiseDomainEvent(new StudentBeltChangedEvent(Id, beltRankId, awardedDate, note));

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
