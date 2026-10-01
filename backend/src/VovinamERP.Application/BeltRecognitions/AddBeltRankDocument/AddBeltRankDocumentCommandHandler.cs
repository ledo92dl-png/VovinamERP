using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.AddBeltRankDocument;

public sealed class AddBeltRankDocumentCommandHandler
    : IRequestHandler<AddBeltRankDocumentCommand, Result<Guid>>
{
    private const int CertificateMinimumLevel = 2;
    private const int CertificateMaximumLevel = 5;
    private const int RankDiplomaMinimumLevel = 6;

    private readonly IRepository<BeltRankRecognition>
        _recognitionRepository;

    private readonly IRepository<BeltRank>
        _beltRankRepository;

    private readonly IRepository<BeltRankDocument>
        _documentRepository;

    private readonly IUnitOfWork _unitOfWork;

    public AddBeltRankDocumentCommandHandler(
        IRepository<BeltRankRecognition> recognitionRepository,
        IRepository<BeltRank> beltRankRepository,
        IRepository<BeltRankDocument> documentRepository,
        IUnitOfWork unitOfWork)
    {
        _recognitionRepository = recognitionRepository;
        _beltRankRepository = beltRankRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddBeltRankDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var recognition =
            await _recognitionRepository.GetByIdAsync(
                request.BeltRankRecognitionId,
                cancellationToken);

        if (recognition is null ||
            recognition.IsArchived ||
            recognition.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_APP_001",
                "Belt rank recognition was not found.");
        }

        var beltRank =
            await _beltRankRepository.GetByIdAsync(
                recognition.BeltRankId,
                cancellationToken);

        if (beltRank is null ||
            beltRank.IsArchived ||
            !beltRank.IsActive)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_APP_002",
                "Belt rank was not found or is inactive.");
        }

        if (beltRank.Level < CertificateMinimumLevel)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_APP_004",
                "This belt rank does not support a rank document.");
        }

        var expectedDocumentType =
            beltRank.Level <= CertificateMaximumLevel
                ? BeltRankDocumentType.Certificate
                : BeltRankDocumentType.RankDiploma;

        if (beltRank.Level >= RankDiplomaMinimumLevel)
        {
            expectedDocumentType =
                BeltRankDocumentType.RankDiploma;
        }

        if (request.DocumentType != expectedDocumentType)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_APP_003",
                expectedDocumentType ==
                    BeltRankDocumentType.Certificate
                    ? "This belt rank requires a certificate."
                    : "This belt rank requires a rank diploma.");
        }

        var documentExists =
            await _documentRepository.ExistsAsync(
                x =>
                    x.TenantId == request.TenantId &&
                    x.BeltRankRecognitionId ==
                        recognition.Id,
                cancellationToken);

        if (documentExists)
        {
            return Failure(
                "BELT_RANK_DOCUMENT_APP_005",
                "This belt rank recognition already has a document.");
        }

        var documentResult =
            BeltRankDocument.Create(
                request.TenantId,
                recognition.Id,
                request.DocumentType,
                request.DocumentNumber,
                request.SignedDate,
                request.ScanUrl,
                request.Note);

        if (documentResult.IsFailure ||
            documentResult.Value is null)
        {
            return Result<Guid>.Failure(
                documentResult.Error);
        }

        await _documentRepository.AddAsync(
            documentResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            documentResult.Value.Id);
    }

    private static Result<Guid> Failure(
        string code,
        string message)
    {
        return Result<Guid>.Failure(
            new Error(code, message));
    }
}