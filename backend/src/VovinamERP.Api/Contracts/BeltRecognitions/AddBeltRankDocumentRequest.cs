using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Api.Contracts.BeltRecognitions;

public sealed record AddBeltRankDocumentRequest(
    Guid TenantId,
    BeltRankDocumentType DocumentType,
    string DocumentNumber,
    DateOnly SignedDate,
    string? ScanUrl,
    string? Note);