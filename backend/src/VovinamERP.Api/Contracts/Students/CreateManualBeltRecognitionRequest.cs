namespace VovinamERP.Api.Contracts.Students;

public sealed record CreateManualBeltRecognitionRequest(
    Guid TenantId,
    Guid BeltRankId,
    DateOnly RecognitionDate,
    string? Note,
    Guid? UserId);