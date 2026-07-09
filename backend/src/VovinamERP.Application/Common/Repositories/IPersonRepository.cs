using VovinamERP.Domain.Persons;

namespace VovinamERP.Application.Common.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByCodeAsync(
        Guid tenantId,
        string code,
        CancellationToken cancellationToken = default);
}
