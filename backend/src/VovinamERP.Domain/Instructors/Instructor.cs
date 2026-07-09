using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Instructors;

public sealed class Instructor : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PersonId { get; private set; }
    public Guid OrganizationId { get; private set; }

    public string InstructorNumber { get; private set; } = default!;
    public Guid? CurrentBeltRankId { get; private set; }
    public InstructorStatus Status { get; private set; }
    public string? ProfessionalNote { get; private set; }

    private Instructor() { }

    private Instructor(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        string instructorNumber,
        Guid? currentBeltRankId,
        string? professionalNote)
    {
        TenantId = tenantId;
        PersonId = personId;
        OrganizationId = organizationId;
        InstructorNumber = instructorNumber.Trim();
        CurrentBeltRankId = currentBeltRankId;
        ProfessionalNote = professionalNote?.Trim();
        Status = InstructorStatus.Active;

        RaiseDomainEvent(new InstructorCreatedEvent(Id, TenantId, PersonId, InstructorNumber));
    }

    public static Result<Instructor> Create(
        Guid tenantId,
        Guid personId,
        Guid organizationId,
        string instructorNumber,
        Guid? currentBeltRankId,
        string? professionalNote)
    {
        if (tenantId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.TenantRequired);

        if (personId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.PersonRequired);

        if (organizationId == Guid.Empty)
            return Result<Instructor>.Failure(InstructorErrors.OrganizationRequired);

        if (string.IsNullOrWhiteSpace(instructorNumber))
            return Result<Instructor>.Failure(InstructorErrors.InstructorNumberRequired);

        return Result<Instructor>.Success(
            new Instructor(tenantId, personId, organizationId, instructorNumber, currentBeltRankId, professionalNote));
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = InstructorStatus.Archived;
        base.Archive(userId);
        RaiseDomainEvent(new InstructorArchivedEvent(Id));
    }
}
