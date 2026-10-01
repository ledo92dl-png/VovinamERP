using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltRecognitions;

public static class BeltRankDocumentErrors
{
    public static readonly Error TenantRequired =
        new(
            "BELT_RANK_DOCUMENT_001",
            "Tenant is required.");

    public static readonly Error RecognitionRequired =
        new(
            "BELT_RANK_DOCUMENT_002",
            "Belt rank recognition is required.");

    public static readonly Error InvalidDocumentType =
        new(
            "BELT_RANK_DOCUMENT_003",
            "Document type is invalid.");

    public static readonly Error DocumentNumberRequired =
        new(
            "BELT_RANK_DOCUMENT_004",
            "Document number is required.");

    public static readonly Error SignedDateRequired =
        new(
            "BELT_RANK_DOCUMENT_005",
            "Signed date is required.");

    public static readonly Error DocumentNumberTooLong =
        new(
            "BELT_RANK_DOCUMENT_006",
            "Document number cannot exceed 200 characters.");

    public static readonly Error ScanUrlTooLong =
        new(
            "BELT_RANK_DOCUMENT_007",
            "Scan URL cannot exceed 2000 characters.");

    public static readonly Error NoteTooLong =
        new(
            "BELT_RANK_DOCUMENT_008",
            "Note cannot exceed 1024 characters.");
}