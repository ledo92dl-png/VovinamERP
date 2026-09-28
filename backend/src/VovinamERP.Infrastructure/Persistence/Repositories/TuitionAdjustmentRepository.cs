
using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class TuitionAdjustmentRepository
    : ITuitionAdjustmentRepository
{
    private readonly VovinamDbContext _context;

    public TuitionAdjustmentRepository(
        VovinamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TuitionAdjustment adjustment,
        CancellationToken cancellationToken = default)
    {
        await _context.TuitionAdjustments.AddAsync(
            adjustment,
            cancellationToken);
    }

    public Task<List<TuitionAdjustment>> GetByInvoiceIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default)
    {
        return _context.TuitionAdjustments
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.TuitionInvoiceId == tuitionInvoiceId)
            .OrderByDescending(x => x.ApprovedAtUtc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}