using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Instructors;

public sealed class Instructor : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PersonId { get; private set; }
    public string InstructorNumber { get; private set; } = default!;
    public Guid OrganizationId { get; private set; }
    public Guid? CurrentBeltRankId { get; private set; }
    public InstructorStatus Status { get; private set; }
    public string? ProfessionalTitle { get; private set; }
    public string? Note { get; private set; }

    private Instructor() { }

    private Instructor(
        Guid tenantId,
        Guid personId,
        string instructorNumber,
        Guid organizationId,
        Guid? currentBeltRankId,
        string? professionalTitle,
        string? note)
    {
        TenantId = tenantId;
        PersonId = personId;
        InstructorNumber = instructorNumber.Trim();
        OrganizationId = organizationId;
        CurrentBeltRankId = currentBeltRankId;
        ProfessionalTitle = professionalTitle?.Trim();
        Note = note?.Trim();
        Status = InstructorStatus.Active;

        RaiseDomainEvent(new InstructorCreatedEvent(Id, PersonId, InstructorNumber));
    }

    public static Result<Instructor> Create(
        Guid tenantId,
        Guid personId,
        string instructorNumber,
        Guid organizationId,
        Guid? currentBeltRankId,
        string? professionalTitle,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.TenantRequired);

        if (personId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.PersonRequired);

        if (string.IsNullOrWhiteSpace(instructorNumber))
            return Result<Instructor>.Failure(InstructorErrors.InstructorNumberRequired);

        if (organizationId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.OrganizationRequired);

        return Result<Instructor>.Success(new Instructor(
            tenantId,
            personId,
            instructorNumber,
            organizationId,
            currentBeltRankId,
            professionalTitle,
            note));
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = InstructorStatus.Archived;
        base.Archive(userId);
        RaiseDomainEvent(new InstructorArchivedEvent(Id));
    }
}
