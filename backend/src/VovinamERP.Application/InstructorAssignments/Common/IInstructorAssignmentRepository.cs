using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Application.InstructorAssignments.Common;

public interface IInstructorAssignmentRepository
{
    Task<InstructorAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InstructorAssignment>> GetByTrainingClassIdAsync(
        Guid trainingClassId,
        CancellationToken cancellationToken = default);

    Task AddAsync(InstructorAssignment assignment, CancellationToken cancellationToken = default);

    void Update(InstructorAssignment assignment);
}