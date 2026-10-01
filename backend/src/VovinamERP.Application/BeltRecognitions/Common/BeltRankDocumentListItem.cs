using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Application.BeltRecognitions.Common;

public sealed record BeltRankDocumentListItem(
    Guid Id,
    BeltRankDocumentType DocumentType,
    string DocumentNumber,
    DateOnly SignedDate,
    string? ScanUrl,
    string? Note);