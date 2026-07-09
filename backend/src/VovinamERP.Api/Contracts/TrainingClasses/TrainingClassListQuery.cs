using VovinamERP.Domain.Training;

namespace VovinamERP.Api.Contracts.TrainingClasses;

public sealed record TrainingClassListQuery(
    Guid? TenantId,
    Guid? OrganizationId,
    TrainingClassStatus? Status,
    string? Keyword,
    string? SortBy,
    bool Descending = false,
    int Page = 1,
    int PageSize = 20);
