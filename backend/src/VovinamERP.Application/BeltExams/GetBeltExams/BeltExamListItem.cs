namespace VovinamERP.Application.BeltExams.GetBeltExams;

public sealed record BeltExamListItem(
    Guid Id,
    DateOnly ExamDate,
    string SessionName,
    string Location,
    Guid TargetBeltRankId,
    string TargetBeltCode,
    string TargetBeltName,
    string SourceBeltName,
    int StudentCount);