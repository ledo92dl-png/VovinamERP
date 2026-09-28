namespace VovinamERP.Application.Finance.GetStudentCreditHistory;

public sealed record GetStudentCreditHistoryResult(
    Guid StudentId,
    decimal CreditBalance,
    IReadOnlyList<StudentCreditHistoryItem> Transactions);