using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Api.Contracts.InstructorAssignments;

public sealed record AssignInstructorToClassRequest(
    Guid TenantId,
    Guid InstructorId,
    InstructorAssignmentRole Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Note);