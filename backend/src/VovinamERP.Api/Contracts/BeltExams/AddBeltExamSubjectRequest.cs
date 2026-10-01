namespace VovinamERP.Api.Contracts.BeltExams;

public sealed record AddBeltExamSubjectRequest(
    Guid TenantId,
    string Name,
    int DisplayOrder,
    decimal? MaximumScore);