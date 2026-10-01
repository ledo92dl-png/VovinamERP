using Moq;
using VovinamERP.Application.BeltRecognitions.UploadBeltRankDocumentScan;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Tests;

public sealed class UploadBeltRankDocumentScanCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidPdf_ShouldSaveFileAndUpdateDocument()
    {
        var setup = CreateSetup();

        var result = await setup.Handler.Handle(
            CreateCommand(setup),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            setup.NewStoredPath,
            result.Value);
        Assert.Equal(
            setup.NewStoredPath,
            setup.Document.ScanUrl);

        setup.FileStorage.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                "scan.pdf",
                "application/pdf",
                setup.TenantId,
                "belt-rank-documents",
                It.IsAny<CancellationToken>()),
            Times.Once);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WrongTenant_ShouldFailWithoutSavingFile()
    {
        var setup = CreateSetup();

        var command = CreateCommand(
            setup,
            tenantId: Guid.NewGuid());

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_001",
            result.Error.Code);

        VerifyNoFileOrDatabaseSave(setup);
    }

    [Fact]
    public async Task Handle_EmptyFile_ShouldFailWithoutSavingFile()
    {
        var setup = CreateSetup();

        var command = CreateCommand(
            setup,
            fileSize: 0);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_002",
            result.Error.Code);

        VerifyNoFileOrDatabaseSave(setup);
    }

    [Fact]
    public async Task Handle_FileLargerThanTenMegabytes_ShouldFail()
    {
        var setup = CreateSetup();

        var command = CreateCommand(
            setup,
            fileSize:
                (10L * 1024L * 1024L) + 1L);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_003",
            result.Error.Code);

        VerifyNoFileOrDatabaseSave(setup);
    }

    [Fact]
    public async Task Handle_UnsupportedExtension_ShouldFail()
    {
        var setup = CreateSetup();

        var command = CreateCommand(
            setup,
            fileName: "scan.exe",
            contentType:
                "application/octet-stream");

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_005",
            result.Error.Code);

        VerifyNoFileOrDatabaseSave(setup);
    }

    [Fact]
    public async Task Handle_MismatchedContentType_ShouldFail()
    {
        var setup = CreateSetup();

        var command = CreateCommand(
            setup,
            fileName: "scan.pdf",
            contentType: "image/png");

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_006",
            result.Error.Code);

        VerifyNoFileOrDatabaseSave(setup);
    }

    [Fact]
    public async Task Handle_DatabaseFailure_ShouldDeleteNewFile()
    {
        var setup = CreateSetup();

        setup.UnitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database test failure"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => setup.Handler.Handle(
                CreateCommand(setup),
                CancellationToken.None));
                        Assert.Null(
            setup.Document.ScanUrl);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                setup.NewStoredPath,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReplacingExistingScan_ShouldDeleteOldFileAfterSave()
    {
        const string oldStoredPath =
            "/uploads/old/belt-rank-documents/old.pdf";

        var setup = CreateSetup(
            oldStoredPath);

        var result = await setup.Handler.Handle(
            CreateCommand(setup),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            setup.NewStoredPath,
            setup.Document.ScanUrl);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                oldStoredPath,
                CancellationToken.None),
            Times.Once);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                setup.NewStoredPath,
                CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseFailureWhileReplacing_ShouldKeepOldFileAndDeleteNewFile()
    {
        const string oldStoredPath =
            "/uploads/old/belt-rank-documents/old.pdf";

        var setup = CreateSetup(
            oldStoredPath);

        setup.UnitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database test failure"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => setup.Handler.Handle(
                CreateCommand(setup),
                CancellationToken.None));
                        Assert.Equal(
            oldStoredPath,
            setup.Document.ScanUrl);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                setup.NewStoredPath,
                CancellationToken.None),
            Times.Once);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                oldStoredPath,
                CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ActualStoredFileExceedsMaximumSize_ShouldDeleteFileAndFail()
    {
        var setup = CreateSetup();

        setup.FileStorage
            .Setup(x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                setup.TenantId,
                "belt-rank-documents",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StoredFile(
                    setup.NewStoredPath,
                    "scan.pdf",
                    "application/pdf",
                    10L * 1024L * 1024L + 1L));

        var command = CreateCommand(
            setup,
            fileSize: 4);

        var result = await setup.Handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BELT_RANK_DOCUMENT_SCAN_APP_003",
            result.Error.Code);
        Assert.Null(setup.Document.ScanUrl);

        setup.FileStorage.Verify(
            x => x.DeleteAsync(
                setup.NewStoredPath,
                CancellationToken.None),
            Times.Once);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static ScanSetup CreateSetup(
        string? existingScanUrl = null)
    {
        var tenantId = Guid.NewGuid();

        var documentResult =
            BeltRankDocument.Create(
                tenantId,
                Guid.NewGuid(),
                BeltRankDocumentType.RankDiploma,
                "BD-001",
                new DateOnly(2026, 10, 1),
                existingScanUrl,
                null);

        Assert.True(documentResult.IsSuccess);
        Assert.NotNull(documentResult.Value);

        var document = documentResult.Value;

        var documentRepository =
            new Mock<IRepository<BeltRankDocument>>();

        var fileStorage =
            new Mock<IFileStorage>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        documentRepository
            .Setup(x => x.GetByIdAsync(
                document.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var newStoredPath =
            $"/uploads/{tenantId:D}/belt-rank-documents/new.pdf";

        fileStorage
            .Setup(x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                tenantId,
                "belt-rank-documents",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StoredFile(
                    newStoredPath,
                    "scan.pdf",
                    "application/pdf",
                    4));

        fileStorage
            .Setup(x => x.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler =
            new UploadBeltRankDocumentScanCommandHandler(
                documentRepository.Object,
                fileStorage.Object,
                unitOfWork.Object);

        return new ScanSetup(
            tenantId,
            document,
            newStoredPath,
            documentRepository,
            fileStorage,
            unitOfWork,
            handler);
    }

    private static UploadBeltRankDocumentScanCommand CreateCommand(
        ScanSetup setup,
        Guid? tenantId = null,
        string fileName = "scan.pdf",
        string contentType = "application/pdf",
        long fileSize = 4)
    {
        return new UploadBeltRankDocumentScanCommand(
            tenantId ?? setup.TenantId,
            setup.Document.Id,
            new MemoryStream([1, 2, 3, 4]),
            fileName,
            contentType,
            fileSize,
            null);
    }

    private static void VerifyNoFileOrDatabaseSave(
        ScanSetup setup)
    {
        setup.FileStorage.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        setup.UnitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private sealed record ScanSetup(
        Guid TenantId,
        BeltRankDocument Document,
        string NewStoredPath,
        Mock<IRepository<BeltRankDocument>>
            DocumentRepository,
        Mock<IFileStorage> FileStorage,
        Mock<IUnitOfWork> UnitOfWork,
        UploadBeltRankDocumentScanCommandHandler Handler);
}
