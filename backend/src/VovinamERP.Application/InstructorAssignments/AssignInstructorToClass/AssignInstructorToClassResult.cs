namespace VovinamERP.Application.InstructorAssignments.AssignInstructorToClass;

public sealed record AssignInstructorToClassResult(
    Guid AssignmentId,
    Guid TrainingClassId,
    Guid InstructorId,
    string Message);