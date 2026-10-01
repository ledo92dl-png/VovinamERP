using Microsoft.AspNetCore.Http;

namespace VovinamERP.Api.Contracts.BeltRecognitions;

public sealed class UploadBeltRankDocumentScanRequest
{
    public Guid TenantId { get; init; }

    public IFormFile File { get; init; } = default!;
}