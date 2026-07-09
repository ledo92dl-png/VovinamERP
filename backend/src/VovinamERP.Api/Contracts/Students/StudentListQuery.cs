using VovinamERP.Domain.Students;

namespace VovinamERP.Api.Contracts.Students;

public sealed record StudentListQuery(
    Guid? TenantId,
    Guid? OrganizationId,
    Guid? CurrentBeltRankId,
    StudentStatus? Status,
    string? Keyword,
    string? SortBy,
    bool Descending = false,
    int Page = 1,
    int PageSize = 20);
