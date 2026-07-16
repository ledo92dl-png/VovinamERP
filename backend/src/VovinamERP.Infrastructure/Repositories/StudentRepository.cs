using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly VovinamDbContext _context;

    public StudentRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Student>()
            .FirstOrDefaultAsync(
                x => x.Id == studentId &&
                     x.TenantId == tenantId &&
                     !x.IsArchived,
                cancellationToken);
    }

    public async Task<Student?> GetByQrTokenAsync(
        Guid tenantId,
        string qrToken,
        CancellationToken cancellationToken = default)
    {
        var normalizedToken = qrToken.Trim().ToLowerInvariant();

        return await _context.Set<Student>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.QrToken == normalizedToken &&
                     !x.IsArchived,
                cancellationToken);
    }

    public void Update(Student student)
    {
        _context.Set<Student>().Update(student);
    }
}