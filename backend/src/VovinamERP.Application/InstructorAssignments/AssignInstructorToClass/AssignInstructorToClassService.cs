using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Application.InstructorAssignments.AssignInstructorToClass;

public sealed class AssignInstructorToClassService
{
    public Task<AssignInstructorToClassResult> HandleAsync(
        AssignInstructorToClassCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignmentResult = InstructorAssignment.Assign(
            command.TenantId,
            command.TrainingClassId,
            command.InstructorId,
            command.Role,
            command.StartDate,
            command.EndDate,
            command.Note);

        if (assignmentResult.IsFailure || assignmentResult.Value is null)
            throw new InvalidOperationException(assignmentResult.Error.Message);

        var assignment = assignmentResult.Value;

        var result = new AssignInstructorToClassResult(
            assignment.Id,
            assignment.TrainingClassId,
            assignment.InstructorId,
            "Instructor assigned successfully.");

        return Task.FromResult(result);
    }
}