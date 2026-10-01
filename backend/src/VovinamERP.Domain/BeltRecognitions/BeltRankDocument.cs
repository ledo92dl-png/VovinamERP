using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltRecognitions;

public sealed class BeltRankDocument : AggregateRoot
{
    public Guid TenantId { get; private set; }

    public Guid BeltRankRecognitionId { get; private set; }

    public BeltRankDocumentType DocumentType { get; private set; }

    public string DocumentNumber { get; private set; } = default!;

    public DateOnly SignedDate { get; private set; }

    public string? ScanUrl { get; private set; }

    public string? Note { get; private set; }

    private BeltRankDocument()
    {
    }

    private BeltRankDocument(
        Guid tenantId,
        Guid beltRankRecognitionId,
        BeltRankDocumentType documentType,
        string documentNumber,
        DateOnly signedDate,
        string? scanUrl,
        string? note)
    {
        TenantId = tenantId;
        BeltRankRecognitionId = beltRankRecognitionId;
        DocumentType = documentType;
        DocumentNumber = documentNumber.Trim();
        SignedDate = signedDate;
        ScanUrl = scanUrl?.Trim();
        Note = note?.Trim();
    }

    public static Result<BeltRankDocument> Create(
        Guid tenantId,
        Guid beltRankRecognitionId,
        BeltRankDocumentType documentType,
        string documentNumber,
        DateOnly signedDate,
        string? scanUrl,
        string? note)
    {
        if (tenantId == Guid.Empty)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.TenantRequired);
        }

        if (beltRankRecognitionId == Guid.Empty)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.RecognitionRequired);
        }

        if (!Enum.IsDefined(documentType))
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.InvalidDocumentType);
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.DocumentNumberRequired);
        }

        if (signedDate == default)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.SignedDateRequired);
        }

        if (documentNumber.Trim().Length > 200)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.DocumentNumberTooLong);
        }

        if (scanUrl?.Trim().Length > 2000)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.ScanUrlTooLong);
        }

        if (note?.Trim().Length > 1024)
        {
            return Result<BeltRankDocument>.Failure(
                BeltRankDocumentErrors.NoteTooLong);
        }

        return Result<BeltRankDocument>.Success(
            new BeltRankDocument(
                tenantId,
                beltRankRecognitionId,
                documentType,
                documentNumber,
                signedDate,
                scanUrl,
                note));
    }
}