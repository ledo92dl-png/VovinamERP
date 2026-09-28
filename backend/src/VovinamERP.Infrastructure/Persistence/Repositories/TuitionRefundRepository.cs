
using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class TuitionRefundRepository
    : ITuitionRefundRepository
{
    private readonly VovinamDbContext _context;

    public TuitionRefundRepository(
        VovinamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TuitionRefund refund,
        CancellationToken cancellationToken = default)
    {
        await _context.TuitionRefunds.AddAsync(
            refund,
            cancellationToken);
    }

    public async Task<TuitionRefund?> GetByIdAsync(
        Guid tenantId,
        Guid refundId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TuitionRefunds
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.Id == refundId,
                cancellationToken);
    }

    public async Task<TuitionRefund?> GetByAdjustmentIdAsync(
        Guid tenantId,
        Guid tuitionAdjustmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TuitionRefunds
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.TuitionAdjustmentId == tuitionAdjustmentId,
                cancellationToken);
    }

    public async Task<List<TuitionRefund>> GetByInvoiceIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TuitionRefunds
            .AsNoTracking()
            .Where(
                x => x.TenantId == tenantId &&
                     x.TuitionInvoiceId == tuitionInvoiceId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}