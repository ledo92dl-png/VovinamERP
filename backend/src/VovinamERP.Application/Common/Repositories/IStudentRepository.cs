using VovinamERP.Domain.Students;

namespace VovinamERP.Application.Common.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByMemberNumberAsync(
        Guid tenantId,
        string memberNumber,
        CancellationToken cancellationToken = default);
}
