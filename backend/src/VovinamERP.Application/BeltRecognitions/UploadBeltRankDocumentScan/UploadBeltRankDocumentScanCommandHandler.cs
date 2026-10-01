using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.UploadBeltRankDocumentScan;

public sealed class UploadBeltRankDocumentScanCommandHandler
    : IRequestHandler<UploadBeltRankDocumentScanCommand, Result<string>>
{
    private const long MaximumFileSize =
        10L * 1024L * 1024L;

    private const string StorageCategory =
        "belt-rank-documents";

    private static readonly IReadOnlyDictionary<string, string[]>
        AllowedFileTypes =
            new Dictionary<string, string[]>(
                StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] =
                    ["image/jpeg"],
                [".jpeg"] =
                    ["image/jpeg"],
                [".png"] =
                    ["image/png"],
                [".webp"] =
                    ["image/webp"],
                [".pdf"] =
                    ["application/pdf"]
            };

    private readonly IRepository<BeltRankDocument>
        _documentRepository;

    private readonly IFileStorage _fileStorage;

    private readonly IUnitOfWork _unitOfWork;

    public UploadBeltRankDocumentScanCommandHandler(
        IRepository<BeltRankDocument> documentRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(
        UploadBeltRankDocumentScanCommand request,
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
                "BELT_RANK_DOCUMENT_SCAN_APP_001",
                "Belt rank document was not found.");
        }

        if (request.Content is null ||
            request.FileSize <= 0)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_APP_002",
                "Scan file is required.");
        }

        if (request.FileSize > MaximumFileSize)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_APP_003",
                "Scan file cannot exceed 10 MB.");
        }

        if (string.IsNullOrWhiteSpace(
                request.FileName))
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_APP_004",
                "Scan file name is required.");
        }

        var extension = Path
            .GetExtension(
                Path.GetFileName(request.FileName))
            .ToLowerInvariant();

        if (!AllowedFileTypes.TryGetValue(
                extension,
                out var allowedContentTypes))
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_APP_005",
                "Only JPG, JPEG, PNG, WebP, and PDF files are allowed.");
        }

        var contentType =
            request.ContentType?.Trim();

        if (string.IsNullOrWhiteSpace(contentType) ||
            !allowedContentTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
        {
            return Failure(
                "BELT_RANK_DOCUMENT_SCAN_APP_006",
                "File extension and content type do not match.");
        }

        var previousScanUrl =
            document.ScanUrl;

        StoredFile? storedFile = null;

        try
        {
            storedFile = await _fileStorage.SaveAsync(
                request.Content,
                request.FileName,
                contentType,
                request.TenantId,
                StorageCategory,
                cancellationToken);

            if (storedFile.Size > MaximumFileSize)
            {
                await _fileStorage.DeleteAsync(
                    storedFile.StoredPath,
                    CancellationToken.None);

                return Failure(
                    "BELT_RANK_DOCUMENT_SCAN_APP_003",
                    "Scan file cannot exceed 10 MB.");
            }

            var updateResult =
                document.UpdateScan(
                    storedFile.StoredPath);

            if (updateResult.IsFailure)
            {
                await _fileStorage.DeleteAsync(
                    storedFile.StoredPath,
                    CancellationToken.None);

                return Result<string>.Failure(
                    updateResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(
                    previousScanUrl) &&
                !string.Equals(
                    previousScanUrl,
                    storedFile.StoredPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        previousScanUrl,
                        CancellationToken.None);
                }
                catch
                {
                    // The database already points to the new scan.
                    // Failure to clean up the old file must not
                    // invalidate the successful upload.
                }
            }

            return Result<string>.Success(
                storedFile.StoredPath);
        }
        catch
        {
            document.UpdateScan(
                previousScanUrl);

            if (storedFile is not null)
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        storedFile.StoredPath,
                        CancellationToken.None);
                }
                catch
                {
                    // Preserve the original exception.
                }
            }

            throw;
        }

    }

    private static Result<string> Failure(
        string code,
        string message)
    {
        return Result<string>.Failure(
            new Error(code, message));
    }
}
