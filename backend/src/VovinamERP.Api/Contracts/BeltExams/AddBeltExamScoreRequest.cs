namespace VovinamERP.Api.Contracts.BeltExams;

public sealed record AddBeltExamScoreRequest(
    Guid TenantId,
    Guid BeltExamSubjectId,
    decimal Score);
