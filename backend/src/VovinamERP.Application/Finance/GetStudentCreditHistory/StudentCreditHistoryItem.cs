using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Finance.GetStudentCreditHistory;

public sealed record StudentCreditHistoryItem(
    Guid TransactionId,
    StudentCreditTransactionType TransactionType,
    decimal Amount,
    DateOnly TransactionDate,
    Guid? ReceiptId,
    Guid? ReceiptItemId,
    Guid? TuitionInvoiceId,
    string? Description);