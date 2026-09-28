using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Training;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.CreateTrainingSession;

public sealed class CreateTrainingSessionCommandHandler
    : IRequestHandler<CreateTrainingSessionCommand, Result<Guid>>
{
    private readonly IRepository<TrainingClass> _trainingClassRepository;
    private readonly IRepository<TrainingSession> _trainingSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingSessionCommandHandler(
        IRepository<TrainingClass> trainingClassRepository,
        IRepository<TrainingSession> trainingSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingClassRepository = trainingClassRepository;
        _trainingSessionRepository = trainingSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateTrainingSessionCommand request,
        CancellationToken cancellationToken)
    {
        var trainingClass = await _trainingClassRepository.GetByIdAsync(
            request.TrainingClassId,
            cancellationToken);

        if (trainingClass is null ||
            trainingClass.TenantId != request.TenantId ||
            trainingClass.IsArchived)
        {
            return Result<Guid>.Failure(
                new Error(
                    "TRAINING_030",
                    "Training class was not found for the specified tenant."));
        }

        var sessionExists = await _trainingSessionRepository.ExistsAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.TrainingClassId == request.TrainingClassId &&
                x.SessionDate == request.SessionDate &&
                x.StartTime == request.StartTime &&
                !x.IsArchived,
            cancellationToken);

        if (sessionExists)
        {
            return Result<Guid>.Failure(
                new Error(
                    "TRAINING_031",
                    "A training session already exists for this class at the specified date and start time."));
        }

        var createResult = TrainingSession.Create(
            request.TenantId,
            request.TrainingClassId,
            request.SessionDate,
            request.StartTime,
            request.EndTime,
            request.LessonPlan,
            request.CoachNote);

        if (createResult.IsFailure || createResult.Value is null)
        {
            return Result<Guid>.Failure(
                createResult.Error);
        }

        var trainingSession = createResult.Value;

        await _trainingSessionRepository.AddAsync(
            trainingSession,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            trainingSession.Id);
    }
}