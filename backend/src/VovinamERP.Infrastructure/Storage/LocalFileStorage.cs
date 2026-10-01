using VovinamERP.Application.Common.Interfaces;

namespace VovinamERP.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException(
                "Storage root path is required.",
                nameof(rootPath));
        }

        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        Guid tenantId,
        string category,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant is required.",
                nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Content type is required.",
                nameof(contentType));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException(
                "Storage category is required.",
                nameof(category));
        }

        var safeCategory = SanitizePathSegment(category);

        var extension = Path
            .GetExtension(Path.GetFileName(fileName))
            .ToLowerInvariant();

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var relativeDirectory = Path.Combine(
            tenantId.ToString("D"),
            safeCategory);

        var absoluteDirectory = Path.Combine(
            _rootPath,
            relativeDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        var absolutePath = Path.GetFullPath(
            Path.Combine(
                absoluteDirectory,
                storedFileName));

        EnsureInsideRoot(absolutePath);

        await using (var destination = new FileStream(
            absolutePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true))
        {
            await content.CopyToAsync(
                destination,
                cancellationToken);
        }

        var size = new FileInfo(absolutePath).Length;

        var storedPath = "/" + Path
            .Combine(
                "uploads",
                relativeDirectory,
                storedFileName)
            .Replace('\\', '/');

        return new StoredFile(
            storedPath,
            Path.GetFileName(fileName),
            contentType.Trim(),
            size);
    }

    public Task<StoredFileRead?> OpenReadAsync(
        string storedPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storedPath))
        {
            throw new ArgumentException(
                "Stored path is required.",
                nameof(storedPath));
        }

        const string prefix = "/uploads/";

        if (!storedPath.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Stored path is invalid.",
                nameof(storedPath));
        }

        var relativePath = storedPath[prefix.Length..]
            .Replace('/', Path.DirectorySeparatorChar);

        var absolutePath = Path.GetFullPath(
            Path.Combine(
                _rootPath,
                relativePath));

        EnsureInsideRoot(absolutePath);

        if (!File.Exists(absolutePath))
        {
            return Task.FromResult<StoredFileRead?>(null);
        }

        var extension = Path
            .GetExtension(absolutePath)
            .ToLowerInvariant();

        var contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var stream = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        StoredFileRead result = new(
            stream,
            Path.GetFileName(absolutePath),
            contentType,
            stream.Length);

        return Task.FromResult<StoredFileRead?>(
            result);
    }

    public Task DeleteAsync(
        string storedPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return Task.CompletedTask;
        }

        const string prefix = "/uploads/";

        if (!storedPath.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Stored path is invalid.",
                nameof(storedPath));
        }

        var relativePath = storedPath[prefix.Length..]
            .Replace('/', Path.DirectorySeparatorChar);

        var absolutePath = Path.GetFullPath(
            Path.Combine(
                _rootPath,
                relativePath));

        EnsureInsideRoot(absolutePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizePathSegment(
        string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 0 ||
            trimmed == "." ||
            trimmed == ".." ||
            trimmed.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0 ||
            trimmed.Contains('/') ||
            trimmed.Contains('\\'))
        {
            throw new ArgumentException(
                "Storage category is invalid.",
                nameof(value));
        }

        return trimmed;
    }

    private void EnsureInsideRoot(
        string absolutePath)
    {
        var rootWithSeparator =
            _rootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!absolutePath.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Storage path is outside the configured root.");
        }
    }
}
