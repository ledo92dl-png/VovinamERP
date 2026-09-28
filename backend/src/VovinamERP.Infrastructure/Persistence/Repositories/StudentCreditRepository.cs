using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Domain.Finance;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class StudentCreditRepository
    : IStudentCreditRepository
{
    private readonly VovinamDbContext _context;

    public StudentCreditRepository(
        VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetBalanceAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var credit = await _context.StudentCreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.StudentId == studentId &&
                x.TransactionType ==
                    StudentCreditTransactionType.Credit)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken) ?? 0m;

        var debit = await _context.StudentCreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.StudentId == studentId &&
                x.TransactionType ==
                    StudentCreditTransactionType.Debit)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken) ?? 0m;

        return credit - debit;
    }

    public Task<List<StudentCreditTransaction>> GetTransactionsAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return _context.StudentCreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.StudentId == studentId)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    
public Task<List<StudentCreditTransaction>> GetByInvoiceIdAsync(
    Guid tenantId,
    Guid studentId,
    Guid tuitionInvoiceId,
    CancellationToken cancellationToken = default)
{
    return _context.StudentCreditTransactions
        .AsNoTracking()
        .Where(x =>
            x.TenantId == tenantId &&
            x.StudentId == studentId &&
            x.TuitionInvoiceId == tuitionInvoiceId)
        .OrderBy(x => x.TransactionDate)
        .ThenBy(x => x.Id)
        .ToListAsync(cancellationToken);
}
    public async Task AddAsync(
        StudentCreditTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.StudentCreditTransactions.AddAsync(
            transaction,
            cancellationToken);
    }
}