namespace VovinamERP.Api.Contracts.BeltExams;

public sealed record CreateBeltExamRequest(
    Guid TenantId,
    Guid TargetBeltRankId,
    DateOnly ExamDate,
    string SessionName,
    string Location,
    string SourceBeltName,
    string? Note);