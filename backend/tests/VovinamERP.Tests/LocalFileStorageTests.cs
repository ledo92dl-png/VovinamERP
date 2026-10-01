using System.Text;
using VovinamERP.Infrastructure.Storage;

namespace VovinamERP.Tests;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _rootPath;

    public LocalFileStorageTests()
    {
        _rootPath = Path.Combine(
            Path.GetTempPath(),
            "VovinamERP.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_rootPath);
    }

    [Fact]
    public async Task SaveAsync_ShouldStoreFileInTenantAndCategoryDirectory()
    {
        var storage = new LocalFileStorage(_rootPath);
        var tenantId = Guid.NewGuid();
        var bytes = Encoding.UTF8.GetBytes("vovinam-test-content");

        await using var stream = new MemoryStream(bytes);

        var result = await storage.SaveAsync(
            stream,
            "bang-dang-cap.pdf",
            "application/pdf",
            tenantId,
            "belt-rank-documents");

        Assert.Equal(
            "bang-dang-cap.pdf",
            result.FileName);

        Assert.Equal(
            "application/pdf",
            result.ContentType);

        Assert.Equal(
            bytes.LongLength,
            result.Size);

        var prefix =
            $"/uploads/{tenantId:D}/belt-rank-documents/";

        Assert.StartsWith(
            prefix,
            result.StoredPath);

        Assert.EndsWith(
            ".pdf",
            result.StoredPath,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "bang-dang-cap",
            Path.GetFileName(result.StoredPath));

        var absolutePath = ToAbsolutePath(
            result.StoredPath);

        Assert.True(File.Exists(absolutePath));

        var savedBytes =
            await File.ReadAllBytesAsync(absolutePath);

        Assert.Equal(bytes, savedBytes);
    }

    [Fact]
    public async Task SaveAsync_ShouldGenerateDifferentPhysicalNames()
    {
        var storage = new LocalFileStorage(_rootPath);
        var tenantId = Guid.NewGuid();

        await using var firstStream =
            new MemoryStream([1, 2, 3]);

        await using var secondStream =
            new MemoryStream([4, 5, 6]);

        var first = await storage.SaveAsync(
            firstStream,
            "scan.pdf",
            "application/pdf",
            tenantId,
            "belt-rank-documents");

        var second = await storage.SaveAsync(
            secondStream,
            "scan.pdf",
            "application/pdf",
            tenantId,
            "belt-rank-documents");

        Assert.NotEqual(
            first.StoredPath,
            second.StoredPath);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteStoredFile()
    {
        var storage = new LocalFileStorage(_rootPath);
        var tenantId = Guid.NewGuid();

        await using var stream =
            new MemoryStream([1, 2, 3]);

        var stored = await storage.SaveAsync(
            stream,
            "scan.png",
            "image/png",
            tenantId,
            "belt-rank-documents");

        var absolutePath =
            ToAbsolutePath(stored.StoredPath);

        Assert.True(File.Exists(absolutePath));

        await storage.DeleteAsync(
            stored.StoredPath);

        Assert.False(File.Exists(absolutePath));
    }

    [Theory]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("../outside")]
    [InlineData("folder/subfolder")]
    [InlineData(@"folder\subfolder")]
    public async Task SaveAsync_ShouldRejectInvalidCategory(
        string category)
    {
        var storage = new LocalFileStorage(_rootPath);

        await using var stream =
            new MemoryStream([1]);

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.SaveAsync(
                stream,
                "scan.pdf",
                "application/pdf",
                Guid.NewGuid(),
                category));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRejectPathOutsideUploads()
    {
        var storage = new LocalFileStorage(_rootPath);

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.DeleteAsync(
                "/other/location/file.pdf"));
    }

    private string ToAbsolutePath(
        string storedPath)
    {
        const string prefix = "/uploads/";

        Assert.StartsWith(
            prefix,
            storedPath);

        var relativePath =
            storedPath[prefix.Length..]
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        return Path.Combine(
            _rootPath,
            relativePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(
                _rootPath,
                recursive: true);
        }
    }
}