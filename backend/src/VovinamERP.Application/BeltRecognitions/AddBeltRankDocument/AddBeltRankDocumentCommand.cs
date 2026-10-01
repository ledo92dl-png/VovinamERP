using MediatR;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.AddBeltRankDocument;

public sealed record AddBeltRankDocumentCommand(
    Guid TenantId,
    Guid BeltRankRecognitionId,
    BeltRankDocumentType DocumentType,
    string DocumentNumber,
    DateOnly SignedDate,
    string? ScanUrl,
    string? Note,
    Guid? UserId)
    : IRequest<Result<Guid>>;