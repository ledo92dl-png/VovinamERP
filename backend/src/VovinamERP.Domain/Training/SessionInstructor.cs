using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class SessionInstructor : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingSessionId { get; private set; }
    public Guid InstructorId { get; private set; }
    public string? RoleNote { get; private set; }

    private SessionInstructor() { }

    private SessionInstructor(Guid tenantId, Guid trainingSessionId, Guid instructorId, string? roleNote)
    {
        TenantId = tenantId;
        TrainingSessionId = trainingSessionId;
        InstructorId = instructorId;
        RoleNote = roleNote?.Trim();

        RaiseDomainEvent(new SessionInstructorAssignedEvent(Id, TrainingSessionId, InstructorId));
    }

    public static Result<SessionInstructor> Create(
        Guid tenantId,
        Guid trainingSessionId,
        Guid instructorId,
        string? roleNote)
    {
        if (tenantId == Guid.Empty)
            return Result<SessionInstructor>.Failure(TrainingErrors.TenantRequired);

        if (trainingSessionId == Guid.Empty)
            return Result<SessionInstructor>.Failure(TrainingErrors.SessionRequired);

        if (instructorId == Guid.Empty)
            return Result<SessionInstructor>.Failure(TrainingErrors.InstructorRequired);

        return Result<SessionInstructor>.Success(new SessionInstructor(tenantId, trainingSessionId, instructorId, roleNote));
    }
}
