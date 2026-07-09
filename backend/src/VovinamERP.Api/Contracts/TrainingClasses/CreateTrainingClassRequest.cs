namespace VovinamERP.Api.Contracts.TrainingClasses;

public sealed record CreateTrainingClassRequest(
    Guid TenantId,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description);
