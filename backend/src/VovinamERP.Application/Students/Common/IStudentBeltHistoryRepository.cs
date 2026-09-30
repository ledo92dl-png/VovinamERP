namespace VovinamERP.Application.Students.Common;

public interface IStudentBeltHistoryRepository
{
    Task<IReadOnlyList<StudentBeltHistoryListItem>> ListByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);
}