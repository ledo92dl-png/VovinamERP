using VovinamERP.Domain.Students;

namespace VovinamERP.Api.Contracts.Students;

public sealed record ChangeStudentStatusRequest(
    Guid TenantId,
    StudentStatus Status,
    string? Reason);