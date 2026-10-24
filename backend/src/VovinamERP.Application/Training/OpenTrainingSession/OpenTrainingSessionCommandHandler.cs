using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Training;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.OpenTrainingSession;

public sealed class OpenTrainingSessionCommandHandler
    : IRequestHandler<OpenTrainingSessionCommand, Result>
{
    private readonly IRepository<TrainingSession> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenTrainingSessionCommandHandler(
        IRepository<TrainingSession> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        OpenTrainingSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
            return Result.Failure(TrainingErrors.TenantRequired);

        if (request.TrainingSessionId == Guid.Empty)
            return Result.Failure(TrainingErrors.SessionRequired);

        var session = await _repository.GetByIdAsync(
            request.TrainingSessionId,
            cancellationToken);

        if (session is null || session.TenantId != request.TenantId)
        {
            return Result.Failure(
                new Error(
                    "TRAINING_032",
                    "Training session was not found for the specified tenant."));
        }

        var result = session.Open();

        if (result.IsFailure)
            return result;

        _repository.Update(session);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
