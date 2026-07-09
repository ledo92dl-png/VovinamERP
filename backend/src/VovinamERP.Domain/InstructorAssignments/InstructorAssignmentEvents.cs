using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.InstructorAssignments;

public sealed record InstructorAssignedToClassEvent(
    Guid AssignmentId,
    Guid TrainingClassId,
    Guid InstructorId,
    InstructorAssignmentRole Role
) : DomainEvent;

public sealed record InstructorAssignmentRoleChangedEvent(
    Guid AssignmentId,
    InstructorAssignmentRole OldRole,
    InstructorAssignmentRole NewRole
) : DomainEvent;

public sealed record InstructorAssignmentEndedEvent(
    Guid AssignmentId,
    DateOnly EndDate
) : DomainEvent;

public sealed record InstructorAssignmentArchivedEvent(
    Guid AssignmentId
) : DomainEvent;