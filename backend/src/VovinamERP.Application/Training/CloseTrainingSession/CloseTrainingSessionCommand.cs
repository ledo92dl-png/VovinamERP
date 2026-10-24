using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.CloseTrainingSession;

public sealed record CloseTrainingSessionCommand(
    Guid TenantId,
    Guid TrainingSessionId)
    : IRequest<Result>;
