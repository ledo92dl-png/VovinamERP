using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.GetBeltRankDocumentScan;

public sealed record GetBeltRankDocumentScanQuery(
    Guid TenantId,
    Guid BeltRankDocumentId)
    : IRequest<Result<BeltRankDocumentScanFile>>;

public sealed record BeltRankDocumentScanFile(
    Stream Content,
    string FileName,
    string ContentType,
    long Size);