using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.OpenTrainingSession;

public sealed record OpenTrainingSessionCommand(
    Guid TenantId,
    Guid TrainingSessionId)
    : IRequest<Result>;
