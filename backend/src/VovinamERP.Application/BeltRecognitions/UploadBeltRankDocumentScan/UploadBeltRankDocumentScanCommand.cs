using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.UploadBeltRankDocumentScan;

public sealed record UploadBeltRankDocumentScanCommand(
    Guid TenantId,
    Guid BeltRankDocumentId,
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize,
    Guid? UserId)
    : IRequest<Result<string>>;