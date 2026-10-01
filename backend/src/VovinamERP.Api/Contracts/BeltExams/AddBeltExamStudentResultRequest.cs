using VovinamERP.Domain.Students;

namespace VovinamERP.Api.Contracts.BeltExams;

public sealed record AddBeltExamStudentResultRequest(
    Guid TenantId,
    Guid StudentId,
    string UnitName,
    string SourceResult,
    StudentBeltResult Result,
    decimal? TotalScore,
    int? Ranking,
    string? Note);
