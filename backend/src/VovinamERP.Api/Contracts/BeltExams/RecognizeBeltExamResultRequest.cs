namespace VovinamERP.Api.Contracts.BeltExams;

public sealed record RecognizeBeltExamResultRequest(
    Guid TenantId,
    DateOnly RecognitionDate,
    string? Note);
