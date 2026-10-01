namespace VovinamERP.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        Guid tenantId,
        string category,
        CancellationToken cancellationToken = default);

    Task<StoredFileRead?> OpenReadAsync(
        string storedPath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storedPath,
        CancellationToken cancellationToken = default);
}

public sealed record StoredFile(
    string StoredPath,
    string FileName,
    string ContentType,
    long Size);

public sealed record StoredFileRead(
    Stream Content,
    string FileName,
    string ContentType,
    long Size);