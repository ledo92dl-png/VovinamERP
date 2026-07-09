using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Application.InstructorAssignments.AssignInstructorToClass;

public sealed record AssignInstructorToClassCommand(
    Guid TenantId,
    Guid TrainingClassId,
    Guid InstructorId,
    InstructorAssignmentRole Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Note);