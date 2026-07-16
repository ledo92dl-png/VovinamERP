using VovinamERP.Domain.Students;

namespace VovinamERP.Application.Students.Common;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<Student?> GetByQrTokenAsync(
        Guid tenantId,
        string qrToken,
        CancellationToken cancellationToken = default);

    void Update(Student student);
}