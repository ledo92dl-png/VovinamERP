using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Common.Repositories;
using VovinamERP.Domain.Students;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(VovinamDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Student?> GetByMemberNumberAsync(
        Guid tenantId,
        string memberNumber,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            student => student.TenantId == tenantId && student.MemberNumber == memberNumber,
            cancellationToken);
    }
}
