using VovinamERP.Domain.Students;

namespace VovinamERP.Application.Students.Common;

public sealed record StudentBeltHistoryListItem(
    Guid Id,
    Guid BeltRankId,
    string BeltCode,
    string BeltName,
    int Level,
    DateOnly ExamDate,
    StudentBeltResult Result,
    DateOnly? AwardedDate,
    string? Note);