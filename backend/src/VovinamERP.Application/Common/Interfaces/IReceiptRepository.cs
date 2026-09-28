using VovinamERP.Domain.Finance;

namespace VovinamERP.Application.Common.Interfaces;

public interface IReceiptRepository
{
    Task AddAsync(
        Receipt receipt,
        CancellationToken cancellationToken);

    Task AddItemAsync(
    ReceiptItem item,
    CancellationToken cancellationToken);

    Task<bool> ExistsByReceiptNumberAsync(
        Guid tenantId,
        string receiptNumber,
        CancellationToken cancellationToken);

    Task<Receipt?> GetByIdAsync(
        Guid receiptId,
        CancellationToken cancellationToken);

        Task<(
    IReadOnlyList<Receipt> Items,
    int TotalCount)>
GetPagedAsync(
    Guid tenantId,
    DateOnly? fromDate,
    DateOnly? toDate,
    ReceiptStatus? status,
    PaymentMethod? paymentMethod,
    Guid? collectedByUserId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken);

    Task<(
    IReadOnlyList<(Receipt Receipt, ReceiptItem Item)> Items,
    int TotalCount,
    decimal TotalAmount)>
GetStudentReceiptHistoryAsync(
    Guid tenantId,
    Guid studentId,
    DateOnly? fromDate,
    DateOnly? toDate,
    ReceiptItemType? itemType,
    ReceiptStatus? receiptStatus,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken);
}