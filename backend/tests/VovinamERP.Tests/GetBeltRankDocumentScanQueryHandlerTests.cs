using Moq;
using VovinamERP.Application.BeltRecognitions.GetBeltRankDocumentScan;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Tests;

public sealed class GetBeltRankDocumentScanQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidDocumentAndStoredFile_ShouldReturnFile()
    {
        const string scanUrl =
            "/uploads/test/belt-rank-documents/scan.pdf";

        var setup = CreateSetup(scanUrl);

        var bytes = new byte[] { 1, 2, 3, 4 };

        setup.FileStorage
            .Setup(x => x.OpenReadAsync(
                scanUrl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StoredFileRead(
                    new MemoryStream(bytes),
                    "scan.pdf",
                    "application/pdf",
                    bytes.LongLength));

        var result = await setup.Handler.Handle(
            new GetBeltRankDocumentScanQuery(
                setup.TenantId,
                setup.Document.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "scan.pdf",
            result.Value.FileName);

        Assert.Equal(
            "application/pdf",
            result.Value.ContentType);

        Assert.Equal(
            bytes.LongLength,
            result.Value.Size);

        await using var content =
            result.Value.Content;

        using var memory =
            new MemoryStream();

        await content.CopyToAsync(memory);

        Assert.Equal(
            bytes,
            memory.ToArray());
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldReturnNotFound()
    {
        const string scanUrl =
            "/uploads/test/belt-rank-documents/scan.pdf";

        var setup = CreateSetup(scanUrl);

        var result = await setup.Handler.Handle(
            new GetBeltRankDocumentScanQuery(
                Guid.NewGuid(),
                setup.Document.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_READ_APP_001",
            result.Error.Code);

        setup.FileStorage.Verify(
            x => x.OpenReadAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DocumentWithoutScan_ShouldFailWithoutOpeningStorage()
    {
        var setup = CreateSetup();

        var result = await setup.Handler.Handle(
            new GetBeltRankDocumentScanQuery(
                setup.TenantId,
                setup.Document.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_READ_APP_002",
            result.Error.Code);

        setup.FileStorage.Verify(
            x => x.OpenReadAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MissingPhysicalFile_ShouldReturnStoredFileNotFound()
    {
        const string scanUrl =
            "/uploads/test/belt-rank-documents/missing.pdf";

        var setup = CreateSetup(scanUrl);

        setup.FileStorage
            .Setup(x => x.OpenReadAsync(
                scanUrl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (StoredFileRead?)null);

        var result = await setup.Handler.Handle(
            new GetBeltRankDocumentScanQuery(
                setup.TenantId,
                setup.Document.Id),
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_READ_APP_003",
            result.Error.Code);
    }

    private static ReadSetup CreateSetup(
        string? scanUrl = null)
    {
        var tenantId = Guid.NewGuid();

        var documentResult =
            BeltRankDocument.Create(
                tenantId,
                Guid.NewGuid(),
                BeltRankDocumentType.RankDiploma,
                "BD-READ-001",
                new DateOnly(2026, 10, 1),
                scanUrl,
                null);

        Assert.True(documentResult.IsSuccess);
        Assert.NotNull(documentResult.Value);

        var document =
            documentResult.Value;

        var documentRepository =
            new Mock<IRepository<BeltRankDocument>>();

        var fileStorage =
            new Mock<IFileStorage>();

        documentRepository
            .Setup(x => x.GetByIdAsync(
                document.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var handler =
            new GetBeltRankDocumentScanQueryHandler(
                documentRepository.Object,
                fileStorage.Object);

        return new ReadSetup(
            tenantId,
            document,
            fileStorage,
            handler);
    }

    private sealed record ReadSetup(
        Guid TenantId,
        BeltRankDocument Document,
        Mock<IFileStorage> FileStorage,
        GetBeltRankDocumentScanQueryHandler Handler);
}