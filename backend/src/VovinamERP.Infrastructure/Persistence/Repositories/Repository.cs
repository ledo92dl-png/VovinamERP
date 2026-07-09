using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Common.Repositories;
using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : AggregateRoot
{
    protected readonly VovinamDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(VovinamDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }
}
