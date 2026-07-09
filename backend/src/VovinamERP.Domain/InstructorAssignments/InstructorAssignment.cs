using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.InstructorAssignments;

public sealed class InstructorAssignment : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingClassId { get; private set; }
    public Guid InstructorId { get; private set; }

    public InstructorAssignmentRole Role { get; private set; }
    public InstructorAssignmentStatus Status { get; private set; }

    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public string? Note { get; private set; }

    private InstructorAssignment()
    {
    }

    private InstructorAssignment(
        Guid tenantId,
        Guid trainingClassId,
        Guid instructorId,
        InstructorAssignmentRole role,
        DateOnly startDate,
        DateOnly? endDate,
        string? note)
    {
        TenantId = tenantId;
        TrainingClassId = trainingClassId;
        InstructorId = instructorId;
        Role = role;
        StartDate = startDate;
        EndDate = endDate;
        Note = note?.Trim();
        Status = endDate.HasValue ? InstructorAssignmentStatus.Ended : InstructorAssignmentStatus.Active;

        RaiseDomainEvent(new InstructorAssignedToClassEvent(Id, TrainingClassId, InstructorId, Role));
    }

    public static Result<InstructorAssignment> Assign(
        Guid tenantId,
        Guid trainingClassId,
        Guid instructorId,
        InstructorAssignmentRole role,
        DateOnly startDate,
        DateOnly? endDate,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<InstructorAssignment>.Failure(InstructorAssignmentErrors.TenantRequired);

        if (trainingClassId == Guid.Empty)
            return Result<InstructorAssignment>.Failure(InstructorAssignmentErrors.TrainingClassRequired);

        if (instructorId == Guid.Empty)
            return Result<InstructorAssignment>.Failure(InstructorAssignmentErrors.InstructorRequired);

        if (startDate == default)
            return Result<InstructorAssignment>.Failure(InstructorAssignmentErrors.StartDateRequired);

        if (endDate.HasValue && endDate.Value < startDate)
            return Result<InstructorAssignment>.Failure(InstructorAssignmentErrors.InvalidDateRange);

        var assignment = new InstructorAssignment(
            tenantId,
            trainingClassId,
            instructorId,
            role,
            startDate,
            endDate,
            note);

        return Result<InstructorAssignment>.Success(assignment);
    }

    public Result ChangeRole(InstructorAssignmentRole newRole, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(InstructorAssignmentErrors.AlreadyArchived);

        if (Status == InstructorAssignmentStatus.Ended)
            return Result.Failure(InstructorAssignmentErrors.AlreadyEnded);

        if (Role == newRole)
            return Result.Success();

        var oldRole = Role;
        Role = newRole;

        MarkUpdated(userId);
        RaiseDomainEvent(new InstructorAssignmentRoleChangedEvent(Id, oldRole, newRole));

        return Result.Success();
    }

    public Result End(DateOnly endDate, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(InstructorAssignmentErrors.AlreadyArchived);

        if (Status == InstructorAssignmentStatus.Ended)
            return Result.Failure(InstructorAssignmentErrors.AlreadyEnded);

        if (endDate < StartDate)
            return Result.Failure(InstructorAssignmentErrors.InvalidDateRange);

        EndDate = endDate;
        Status = InstructorAssignmentStatus.Ended;

        MarkUpdated(userId);
        RaiseDomainEvent(new InstructorAssignmentEndedEvent(Id, endDate));

        return Result.Success();
    }

    public Result UpdateNote(string? note, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(InstructorAssignmentErrors.AlreadyArchived);

        Note = note?.Trim();

        MarkUpdated(userId);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = InstructorAssignmentStatus.Archived;
        base.Archive(userId);

        RaiseDomainEvent(new InstructorAssignmentArchivedEvent(Id));
    }
}