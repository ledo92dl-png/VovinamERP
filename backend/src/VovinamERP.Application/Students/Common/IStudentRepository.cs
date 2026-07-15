using VovinamERP.Domain.Students;

namespace VovinamERP.Application.Students.Common;

public interface IStudentRepository
{
    Task<Student?> GetByQrTokenAsync(
        Guid tenantId,
        string qrToken,
        CancellationToken cancellationToken = default);
}