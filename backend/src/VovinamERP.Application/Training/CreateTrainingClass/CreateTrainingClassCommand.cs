using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.CreateTrainingClass;

public sealed record CreateTrainingClassCommand(
    Guid TenantId,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description)
    : IRequest<Result<Guid>>;