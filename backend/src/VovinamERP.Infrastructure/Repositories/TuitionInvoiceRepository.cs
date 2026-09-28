using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class TuitionInvoiceRepository
    : ITuitionInvoiceRepository
{
    private readonly VovinamDbContext _context;

    public TuitionInvoiceRepository(
        VovinamDbContext context)
    {
        _context = context;
    }

    public Task<TuitionInvoice?> GetByIdAsync(
        Guid tenantId,
        Guid tuitionInvoiceId,
        CancellationToken cancellationToken = default)
    {
        return _context.TuitionInvoices
    .Include(x => x.Payments)
    .FirstOrDefaultAsync(
        x =>
            x.TenantId == tenantId &&
            x.Id == tuitionInvoiceId &&
            !x.IsArchived,
        cancellationToken);
    }

        public Task<List<TuitionInvoice>> GetByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return _context.TuitionInvoices
            .AsNoTracking()
            .Include(x => x.Payments)
            .Where(
                x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    !x.IsArchived)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForStudentMonthAsync(
        Guid tenantId,
        Guid studentId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return _context.TuitionInvoices
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.StudentId == studentId &&
                    x.Year == year &&
                    x.Month == month &&
                    !x.IsArchived,
                cancellationToken);
    }

    public Task<bool> ExistsByInvoiceNumberAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedInvoiceNumber = invoiceNumber.Trim();

        return _context.TuitionInvoices
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.InvoiceNumber == normalizedInvoiceNumber &&
                    !x.IsArchived,
                cancellationToken);
    }

    public async Task AddAsync(
        TuitionInvoice tuitionInvoice,
        CancellationToken cancellationToken = default)
    {
        await _context.TuitionInvoices.AddAsync(
            tuitionInvoice,
            cancellationToken);
    }

    public void Update(
        TuitionInvoice tuitionInvoice)
    {
        _context.TuitionInvoices.Update(tuitionInvoice);
    }
}