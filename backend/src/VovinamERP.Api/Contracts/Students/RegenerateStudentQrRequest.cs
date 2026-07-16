namespace VovinamERP.Api.Contracts.Students;

public sealed record RegenerateStudentQrRequest(
    Guid TenantId,
    Guid RegeneratedByUserId);