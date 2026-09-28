namespace VovinamERP.Application.Finance.GetStudentReceiptHistory;

public sealed record GetStudentReceiptHistoryResult(
    Guid StudentId,
    IReadOnlyCollection<StudentReceiptHistoryItemDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    decimal TotalAmount);