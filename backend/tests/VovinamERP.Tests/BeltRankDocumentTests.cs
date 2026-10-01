using VovinamERP.Domain.BeltRecognitions;

namespace VovinamERP.Tests;

public sealed class BeltRankDocumentTests
{
    [Fact]
    public void Create_ValidCertificate_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var recognitionId = Guid.NewGuid();

        var result = BeltRankDocument.Create(
            tenantId,
            recognitionId,
            BeltRankDocumentType.Certificate,
            "  GCN-001  ",
            new DateOnly(2026, 10, 1),
            "  https://example.test/certificate.pdf  ",
            "  Giấy chứng nhận Lam đai.  ");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.Equal(
            recognitionId,
            result.Value.BeltRankRecognitionId);
        Assert.Equal(
            BeltRankDocumentType.Certificate,
            result.Value.DocumentType);
        Assert.Equal("GCN-001", result.Value.DocumentNumber);
        Assert.Equal(
            new DateOnly(2026, 10, 1),
            result.Value.SignedDate);
        Assert.Equal(
            "https://example.test/certificate.pdf",
            result.Value.ScanUrl);
        Assert.Equal(
            "Giấy chứng nhận Lam đai.",
            result.Value.Note);
    }

    [Fact]
    public void Create_ValidRankDiploma_ShouldSucceed()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.RankDiploma,
            "BD-001",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(
            BeltRankDocumentType.RankDiploma,
            result.Value.DocumentType);
    }

    [Fact]
    public void Create_EmptyTenant_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.Empty,
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            "GCN-001",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.TenantRequired,
            result.Error);
    }

    [Fact]
    public void Create_EmptyRecognition_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.Empty,
            BeltRankDocumentType.Certificate,
            "GCN-001",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.RecognitionRequired,
            result.Error);
    }

    [Fact]
    public void Create_InvalidDocumentType_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            (BeltRankDocumentType)999,
            "GCN-001",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.InvalidDocumentType,
            result.Error);
    }

    [Fact]
    public void Create_EmptyDocumentNumber_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            "   ",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.DocumentNumberRequired,
            result.Error);
    }

    [Fact]
    public void Create_DefaultSignedDate_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            "GCN-001",
            default,
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.SignedDateRequired,
            result.Error);
    }

    [Fact]
    public void Create_TooLongDocumentNumber_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            new string('A', 201),
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.DocumentNumberTooLong,
            result.Error);
    }

    [Fact]
    public void Create_TooLongScanUrl_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            "GCN-001",
            new DateOnly(2026, 10, 1),
            new string('A', 2001),
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.ScanUrlTooLong,
            result.Error);
    }

    [Fact]
    public void Create_TooLongNote_ShouldFail()
    {
        var result = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.Certificate,
            "GCN-001",
            new DateOnly(2026, 10, 1),
            null,
            new string('A', 1025));

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.NoteTooLong,
            result.Error);
    }

    [Fact]
    public void UpdateScan_ValidUrl_ShouldUpdateAndTrim()
    {
        var createResult = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.RankDiploma,
            "BD-001",
            new DateOnly(2026, 10, 1),
            null,
            null);

        Assert.True(createResult.IsSuccess);

        var document = createResult.Value!;

        var result = document.UpdateScan(
            "  /uploads/tenant/belt-rank-documents/scan.pdf  ");

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "/uploads/tenant/belt-rank-documents/scan.pdf",
            document.ScanUrl);
    }

    [Fact]
    public void UpdateScan_TooLongUrl_ShouldFailAndPreserveExistingValue()
    {
        var createResult = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.RankDiploma,
            "BD-001",
            new DateOnly(2026, 10, 1),
            "/uploads/original.pdf",
            null);

        Assert.True(createResult.IsSuccess);

        var document = createResult.Value!;

        var result = document.UpdateScan(
            new string('A', 2001));

        Assert.True(result.IsFailure);
        Assert.Equal(
            BeltRankDocumentErrors.ScanUrlTooLong,
            result.Error);
        Assert.Equal(
            "/uploads/original.pdf",
            document.ScanUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateScan_EmptyValue_ShouldClearScanUrl(
        string? scanUrl)
    {
        var createResult = BeltRankDocument.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BeltRankDocumentType.RankDiploma,
            "BD-001",
            new DateOnly(2026, 10, 1),
            "/uploads/original.pdf",
            null);

        Assert.True(createResult.IsSuccess);

        var document = createResult.Value!;

        var result = document.UpdateScan(scanUrl);

        Assert.True(result.IsSuccess);
        Assert.Null(document.ScanUrl);
    }
}