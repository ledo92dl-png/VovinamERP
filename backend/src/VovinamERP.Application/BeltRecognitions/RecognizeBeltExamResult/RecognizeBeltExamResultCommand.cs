using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.RecognizeBeltExamResult;

public sealed record RecognizeBeltExamResultCommand(
    Guid TenantId,
    Guid BeltExamStudentResultId,
    DateOnly RecognitionDate,
    string? Note,
    Guid? UserId)
    : IRequest<Result<Guid>>;
