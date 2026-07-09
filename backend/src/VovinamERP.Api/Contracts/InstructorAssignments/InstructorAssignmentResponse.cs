using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Api.Contracts.InstructorAssignments;

public sealed record InstructorAssignmentResponse(
    Guid AssignmentId,
    Guid TrainingClassId,
    Guid InstructorId,
    InstructorAssignmentRole Role,
    InstructorAssignmentStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Note);