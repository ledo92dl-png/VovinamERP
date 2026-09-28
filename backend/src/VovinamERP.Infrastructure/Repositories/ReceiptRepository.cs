using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class ReceiptRepository : IReceiptRepository
{
    private readonly VovinamDbContext _context;

    public ReceiptRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Receipt receipt,
        CancellationToken cancellationToken)
    {
        await _context.Receipts.AddAsync(
            receipt,
            cancellationToken);
    }

    public Task<bool> ExistsByReceiptNumberAsync(
        Guid tenantId,
        string receiptNumber,
        CancellationToken cancellationToken)
    {
        var normalizedReceiptNumber =
            receiptNumber.Trim();

        return _context.Receipts
            .AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId &&
                     x.ReceiptNumber == normalizedReceiptNumber &&
                     !x.IsArchived,
                cancellationToken);
    }

    public Task<Receipt?> GetByIdAsync(
        Guid receiptId,
        CancellationToken cancellationToken)
    {
        return _context.Receipts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == receiptId &&
                     !x.IsArchived,
                cancellationToken);
    }

    public async Task AddItemAsync(
    ReceiptItem item,
    CancellationToken cancellationToken)
{
    await _context.ReceiptItems.AddAsync(
        item,
        cancellationToken);
}

    public async Task<(
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
    CancellationToken cancellationToken)
{
    var query = _context.Receipts
        .AsNoTracking()
        .Include(x => x.Items)
        .Where(x =>
            x.TenantId == tenantId &&
            !x.IsArchived);

    if (fromDate.HasValue)
    {
        query = query.Where(
            x => x.ReceiptDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        query = query.Where(
            x => x.ReceiptDate <= toDate.Value);
    }

    if (status.HasValue)
    {
        query = query.Where(
            x => x.Status == status.Value);
    }

    if (paymentMethod.HasValue)
    {
        query = query.Where(
            x => x.PaymentMethod == paymentMethod.Value);
    }

    if (collectedByUserId.HasValue)
    {
        query = query.Where(
            x => x.CollectedByUserId ==
                 collectedByUserId.Value);
    }

    var totalCount = await query.CountAsync(
        cancellationToken);

    var items = await query
        .OrderByDescending(x => x.ReceiptDate)
        .ThenByDescending(x => x.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (items, totalCount);
}

    public async Task<(
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
    CancellationToken cancellationToken)
{
    var query =
        from item in _context.ReceiptItems.AsNoTracking()
        join receipt in _context.Receipts.AsNoTracking()
            on item.ReceiptId equals receipt.Id
        where
            item.TenantId == tenantId &&
            item.StudentId == studentId &&
            receipt.TenantId == tenantId &&
            !receipt.IsArchived
        select new
        {
            Receipt = receipt,
            Item = item
        };

    if (fromDate.HasValue)
    {
        query = query.Where(
            x => x.Receipt.ReceiptDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        query = query.Where(
            x => x.Receipt.ReceiptDate <= toDate.Value);
    }

    if (itemType.HasValue)
    {
        query = query.Where(
            x => x.Item.ItemType == itemType.Value);
    }

    if (receiptStatus.HasValue)
    {
        query = query.Where(
            x => x.Receipt.Status == receiptStatus.Value);
    }

    var totalCount = await query.CountAsync(
        cancellationToken);

    var totalAmount = await query
        .SumAsync(
            x => x.Item.Quantity * x.Item.UnitPrice
                 - x.Item.DiscountAmount,
            cancellationToken);

    var rows = await query
        .OrderByDescending(x => x.Receipt.ReceiptDate)
        .ThenByDescending(x => x.Receipt.Id)
        .ThenByDescending(x => x.Item.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    var items = rows
        .Select(x => (x.Receipt, x.Item))
        .ToList();

    return (
        items,
        totalCount,
        totalAmount);
}

}