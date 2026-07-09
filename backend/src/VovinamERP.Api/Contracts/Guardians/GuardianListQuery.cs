namespace VovinamERP.Api.Contracts.Guardians;

public sealed record GuardianListQuery(
    Guid? TenantId,
    string? Keyword,
    string? SortBy,
    bool Descending = false,
    int Page = 1,
    int PageSize = 20);
