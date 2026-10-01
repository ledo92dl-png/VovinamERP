namespace VovinamERP.Application.BeltExams.GetBeltExamScoreSheet;

public sealed record BeltExamScoreSheetResult(
    Guid BeltExamStudentResultId,
    decimal? RecordedTotalScore,
    decimal CalculatedTotalScore,
    int ScoredSubjectCount,
    int TotalSubjectCount,
    bool IsComplete,
    bool? HasTotalScoreMismatch,
    IReadOnlyList<BeltExamScoreSheetSubjectResult> Subjects);

public sealed record BeltExamScoreSheetSubjectResult(
    Guid SubjectId,
    string SubjectName,
    int DisplayOrder,
    decimal? MaximumScore,
    Guid? ScoreId,
    decimal? Score,
    bool IsScored);
