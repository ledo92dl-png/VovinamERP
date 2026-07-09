using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.InstructorAssignments.Common;
using VovinamERP.Domain.InstructorAssignments;

namespace VovinamERP.Application.InstructorAssignments.AssignInstructorToClass.Handlers;

public sealed class AssignInstructorToClassCommandHandler
    : IRequestHandler<AssignInstructorToClassCommand, AssignInstructorToClassResult>
{
    private readonly IInstructorAssignmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignInstructorToClassCommandHandler(
        IInstructorAssignmentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignInstructorToClassResult> Handle(
        AssignInstructorToClassCommand request,
        CancellationToken cancellationToken)
    {
        var result = InstructorAssignment.Assign(
            request.TenantId,
            request.TrainingClassId,
            request.InstructorId,
            request.Role,
            request.StartDate,
            request.EndDate,
            request.Note);

        if (result.IsFailure || result.Value is null)
            throw new InvalidOperationException(result.Error.Message);

        var assignment = result.Value;

        await _repository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignInstructorToClassResult(
            assignment.Id,
            assignment.TrainingClassId,
            assignment.InstructorId,
            "Instructor assigned successfully.");
    }
}