using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Common.Repositories;
using VovinamERP.Domain.Persons;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public sealed class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(VovinamDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Person?> GetByCodeAsync(
        Guid tenantId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            person => person.TenantId == tenantId && person.Code == code,
            cancellationToken);
    }
}
