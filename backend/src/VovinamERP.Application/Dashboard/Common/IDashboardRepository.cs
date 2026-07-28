using VovinamERP.Application.Dashboard.GetStudentsByBelt;
namespace VovinamERP.Application.Dashboard.Common;

public interface IDashboardRepository
{
    Task<int> CountActiveStudentsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveInstructorsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveOrganizationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<int> CountTrainingSessionsAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default);

    Task<int> CountAttendancesAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default);

    Task<int> CountCrossLocationAttendancesAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default);
        
    Task<(IReadOnlyList<StudentsByBeltItem> Items, int TotalStudents)>
    GetStudentsByBeltAsync(
        Guid tenantId,
        Guid? organizationId,
        CancellationToken cancellationToken = default);
}