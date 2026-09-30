using VovinamERP.Domain.Students;

namespace VovinamERP.Api.Contracts.Students;

public sealed record RecordStudentBeltResultRequest(
    Guid TenantId,
    Guid BeltRankId,
    DateOnly ExamDate,
    StudentBeltResult Result,
    DateOnly? AwardedDate,
    string? Note,
    Guid? UserId);
