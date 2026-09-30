using VovinamERP.Application.Students.Common;

namespace VovinamERP.Application.Students.GetStudentBeltHistory;

public sealed record GetStudentBeltHistoryResult(
    Guid StudentId,
    IReadOnlyList<StudentBeltHistoryListItem> Items);