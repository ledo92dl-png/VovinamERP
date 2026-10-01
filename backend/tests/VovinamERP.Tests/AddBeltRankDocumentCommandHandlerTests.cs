using System.Linq.Expressions;
using Moq;
using VovinamERP.Application.BeltRecognitions.AddBeltRankDocument;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Tests;

public sealed class AddBeltRankDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidLamBeltCertificate_ShouldCreateDocument()
    {
        var setup = CreateSetup(
            "LAM-3",
            "Lam đai III",
            5);

        BeltRankDocument? savedDocument = null;

        setup.DocumentRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltRankDocument>(),
                It.IsAny<CancellationToken>()))
            .Callback<BeltRankDocument, CancellationToken>(
                (document, _) => savedDocument = document)
            .Returns(Task.CompletedTask);

        var result = await setup.Handler.Handle(
            CreateCommand(
                setup,
                BeltRankDocumentType.Certificate),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedDocument);
        Assert.Equal(savedDocument.Id, result.Value);
        Assert.Equal(
            setup.Recognition.Id,
            savedDocument.BeltRankRecognitionId);
        Assert.Equal(
            BeltRankDocumentType.Certificate,
            savedDocument.DocumentType);
        Assert.Equal("DOC-001", savedDocument.DocumentNumber);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidJuniorYellowRankDiploma_ShouldCreateDocument()
    {
        var setup = CreateSetup(
            "HOANG-TN",
            "Hoàng đai thiếu nhi",
            6);

        var result = await setup.Handler.Handle(
            CreateCommand(
                setup,
                BeltRankDocumentType.RankDiploma),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        setup.DocumentRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltRankDocument>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RecognitionNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "LAM",
            "Lam đai",
            2);

        setup.RecognitionRepository
            .Setup(x => x.GetByIdAsync(
                setup.Recognition.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltRankRecognition?)null);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_001",
            BeltRankDocumentType.Certificate);
    }

    [Fact]
    public async Task Handle_WrongTenantRecognition_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "LAM",
            "Lam đai",
            2);

        var command = new AddBeltRankDocumentCommand(
            Guid.NewGuid(),
            setup.Recognition.Id,
            BeltRankDocumentType.Certificate,
            "DOC-001",
            new DateOnly(2026, 10, 1),
            null,
            null,
            null);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_APP_001",
            result.Error.Code);

        VerifyNothingSaved(setup);
    }

    [Fact]
    public async Task Handle_BeltRankNotFound_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "LAM",
            "Lam đai",
            2);

        setup.BeltRankRepository
            .Setup(x => x.GetByIdAsync(
                setup.BeltRank.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeltRank?)null);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_002",
            BeltRankDocumentType.Certificate);
    }

    [Fact]
    public async Task Handle_LamBeltWithRankDiploma_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "LAM-2",
            "Lam đai II",
            4);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_003",
            BeltRankDocumentType.RankDiploma);
    }

    [Fact]
    public async Task Handle_YellowOrHigherWithCertificate_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "HOANG",
            "Hoàng đai",
            6);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_003",
            BeltRankDocumentType.Certificate);
    }

    [Fact]
    public async Task Handle_UnsupportedLevelOneBelt_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "TVNM",
            "Tự vệ nhập môn",
            1);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_004",
            BeltRankDocumentType.Certificate);
    }

    [Fact]
    public async Task Handle_ExistingDocument_ShouldFailWithoutSaving()
    {
        var setup = CreateSetup(
            "HOANG",
            "Hoàng đai",
            6);

        setup.DocumentRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<
                    Expression<Func<BeltRankDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await AssertFailureWithoutSaving(
            setup,
            "BELT_RANK_DOCUMENT_APP_005",
            BeltRankDocumentType.RankDiploma);
    }

    private static DocumentSetup CreateSetup(
        string beltCode,
        string beltName,
        int beltLevel)
    {
        var tenantId = Guid.NewGuid();

        var beltResult = BeltRank.Create(
            beltCode,
            beltName,
            beltLevel,
            null);

        Assert.True(beltResult.IsSuccess);
        Assert.NotNull(beltResult.Value);

        var beltRank = beltResult.Value;

        var recognitionResult = BeltRankRecognition.Create(
            tenantId,
            Guid.NewGuid(),
            beltRank.Id,
            new DateOnly(2026, 9, 30),
            BeltRankRecognitionSource.AgeTransition,
            null,
            "Recognition test");

        Assert.True(recognitionResult.IsSuccess);
        Assert.NotNull(recognitionResult.Value);

        var recognition = recognitionResult.Value;

        var recognitionRepository =
            new Mock<IRepository<BeltRankRecognition>>();

        var beltRankRepository =
            new Mock<IRepository<BeltRank>>();

        var documentRepository =
            new Mock<IRepository<BeltRankDocument>>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        recognitionRepository
            .Setup(x => x.GetByIdAsync(
                recognition.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recognition);

        beltRankRepository
            .Setup(x => x.GetByIdAsync(
                beltRank.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(beltRank);

        documentRepository
            .Setup(x => x.ExistsAsync(
                It.IsAny<
                    Expression<Func<BeltRankDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        documentRepository
            .Setup(x => x.AddAsync(
                It.IsAny<BeltRankDocument>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler =
            new AddBeltRankDocumentCommandHandler(
                recognitionRepository.Object,
                beltRankRepository.Object,
                documentRepository.Object,
                unitOfWork.Object);

        return new DocumentSetup(
            tenantId,
            recognition,
            beltRank,
            recognitionRepository,
            beltRankRepository,
            documentRepository,
            unitOfWork,
            handler);
    }

    private static AddBeltRankDocumentCommand CreateCommand(
        DocumentSetup setup,
        BeltRankDocumentType documentType)
    {
        return new AddBeltRankDocumentCommand(
            setup.TenantId,
            setup.Recognition.Id,
            documentType,
            "DOC-001",
            new DateOnly(2026, 10, 1),
            "https://example.test/document.pdf",
            "Document test",
            Guid.NewGuid());
    }

    private static async Task AssertFailureWithoutSaving(
        DocumentSetup setup,
        string expectedErrorCode,
        BeltRankDocumentType documentType)
    {
        var result = await setup.Handler.Handle(
            CreateCommand(
                setup,
                documentType),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            expectedErrorCode,
            result.Error.Code);

        VerifyNothingSaved(setup);
    }

    private static void VerifyNothingSaved(
        DocumentSetup setup)
    {
        setup.DocumentRepository.Verify(
            x => x.AddAsync(
                It.IsAny<BeltRankDocument>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private sealed record DocumentSetup(
        Guid TenantId,
        BeltRankRecognition Recognition,
        BeltRank BeltRank,
        Mock<IRepository<BeltRankRecognition>>
            RecognitionRepository,
        Mock<IRepository<BeltRank>>
            BeltRankRepository,
        Mock<IRepository<BeltRankDocument>>
            DocumentRepository,
        Mock<IUnitOfWork> UnitOfWork,
        AddBeltRankDocumentCommandHandler Handler);
}