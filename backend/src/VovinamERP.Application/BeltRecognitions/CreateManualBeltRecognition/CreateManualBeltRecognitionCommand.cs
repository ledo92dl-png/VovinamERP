using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.CreateManualBeltRecognition;

public sealed record CreateManualBeltRecognitionCommand(
    Guid TenantId,
    Guid StudentId,
    Guid BeltRankId,
    DateOnly RecognitionDate,
    string? Note,
    Guid? UserId)
    : IRequest<Result<Guid>>;