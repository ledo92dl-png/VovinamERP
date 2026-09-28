namespace VovinamERP.Application.Finance.GetStudentTuitionHistory;

public sealed record GetStudentTuitionHistoryResult(
    Guid StudentId,
    int MonthsRequested,
    IReadOnlyList<StudentTuitionHistoryItem> Items);