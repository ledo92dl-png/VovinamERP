using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.GetBeltRankDocumentScan;

public sealed class GetBeltRankDocumentScanQueryHandler
    : IRequestHandler<
        GetBeltRankDocumentScanQuery,
        Result<BeltRankDocumentScanFile>>
{
    private readonly IRepository<BeltRankDocument>
        _documentRepository;

    private readonly IFileStorage _fileStorage;

    public GetBeltRankDocumentScanQueryHandler(
        IRepository<BeltRankDocument> documentRepository,
        IFileStorage fileStorage)
    {
        _documentRepository = documentRepository;
        _fileStorage = fileStorage;
    }

    public async Task<Result<BeltRankDocumentScanFile>> Handle(
        GetBeltRankDocumentScanQuery request,
        CancellationToken cancellationToken)
    {
        var document =
            await _documentRepository.GetByIdAsync(
                request.BeltRankDocumentId,
                cancellationToken);

        if (document is null ||
            document.IsArchived ||
            document.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_READ_APP_001",
                "Belt rank document was not found.");
        }

        if (string.IsNullOrWhiteSpace(
                document.ScanUrl))
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_READ_APP_002",
                "Belt rank document does not have a scan file.");
        }

        var storedFile =
            await _fileStorage.OpenReadAsync(
                document.ScanUrl,
                cancellationToken);

        if (storedFile is null)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_READ_APP_003",
                "Stored scan file was not found.");
        }

        var file =
            new BeltRankDocumentScanFile(
                storedFile.Content,
                storedFile.FileName,
                storedFile.ContentType,
                storedFile.Size);

        return Result<BeltRankDocumentScanFile>
            .Success(file);
    }

    private static Result<BeltRankDocumentScanFile> Failure(
        string code,
        string message)
    {
        return Result<BeltRankDocumentScanFile>.Failure(
            new Error(code, message));
    }
}