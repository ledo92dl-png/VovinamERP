using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.CreateTrainingSession;

public sealed record CreateTrainingSessionCommand(
    Guid TenantId,
    Guid TrainingClassId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? LessonPlan,
    string? CoachNote)
    : IRequest<Result<Guid>>;