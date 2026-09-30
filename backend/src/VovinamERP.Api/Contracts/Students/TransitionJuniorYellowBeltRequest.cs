namespace VovinamERP.Api.Contracts.Students;

public sealed record TransitionJuniorYellowBeltRequest(
    Guid TenantId,
    DateOnly TransitionDate,
    string? Note,
    Guid? UserId
);