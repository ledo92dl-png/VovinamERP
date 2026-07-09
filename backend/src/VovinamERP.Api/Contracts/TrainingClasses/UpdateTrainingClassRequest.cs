namespace VovinamERP.Api.Contracts.TrainingClasses;

public sealed record UpdateTrainingClassRequest(
    string Name,
    string? Description);
