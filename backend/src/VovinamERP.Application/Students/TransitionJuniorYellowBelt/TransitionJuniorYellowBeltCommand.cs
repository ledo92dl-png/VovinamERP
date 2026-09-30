using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.TransitionJuniorYellowBelt;

public sealed record TransitionJuniorYellowBeltCommand(
    Guid TenantId,
    Guid StudentId,
    DateOnly TransitionDate,
    string? Note,
    Guid? UserId
) : IRequest<Result>;