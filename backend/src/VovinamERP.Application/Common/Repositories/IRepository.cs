using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Application.Common.Repositories;

public interface IRepository<TEntity>
    where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);
}
