namespace VovinamERP.Api.Contracts.Students;

public sealed record ArchiveStudentRequest(
    Guid? UserId,
    string? Reason);
