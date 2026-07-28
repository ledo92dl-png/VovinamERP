using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Dashboard.Common;
using VovinamERP.Domain.Instructors;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Students;
using VovinamERP.Domain.Training;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Application.Dashboard.GetStudentsByBelt;
using VovinamERP.Domain.Belts;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly VovinamDbContext _context;

    public DashboardRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public Task<int> CountActiveStudentsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<Student>()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.Status == StudentStatus.Active &&
                     !x.IsArchived,
                cancellationToken);
    }

    public Task<int> CountActiveInstructorsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<Instructor>()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.Status == InstructorStatus.Active &&
                     !x.IsArchived,
                cancellationToken);
    }

    public Task<int> CountActiveOrganizationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<Organization>()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.Status == OrganizationStatus.Active &&
                     !x.IsArchived,
                cancellationToken);
    }

    public Task<int> CountTrainingSessionsAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<TrainingSession>()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.SessionDate == reportDate &&
                     !x.IsArchived &&
                     x.Status != TrainingSessionStatus.Cancelled &&
                     x.Status != TrainingSessionStatus.Archived,
                cancellationToken);
    }

    public async Task<int> CountAttendancesAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default)
    {
        return await (
            from detail in _context.Set<AttendanceDetail>().AsNoTracking()
            join record in _context.Set<AttendanceRecord>().AsNoTracking()
                on detail.AttendanceRecordId equals record.Id
            join session in _context.Set<TrainingSession>().AsNoTracking()
                on record.TrainingSessionId equals session.Id
            where detail.TenantId == tenantId
                  && record.TenantId == tenantId
                  && session.TenantId == tenantId
                  && session.SessionDate == reportDate
                  && !detail.IsArchived
                  && !record.IsArchived
                  && !session.IsArchived
            select detail.Id
        ).CountAsync(cancellationToken);
    }

    public async Task<int> CountCrossLocationAttendancesAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default)
    {
        return await (
            from detail in _context.Set<AttendanceDetail>().AsNoTracking()
            join record in _context.Set<AttendanceRecord>().AsNoTracking()
                on detail.AttendanceRecordId equals record.Id
            join session in _context.Set<TrainingSession>().AsNoTracking()
                on record.TrainingSessionId equals session.Id
            where detail.TenantId == tenantId
                  && record.TenantId == tenantId
                  && session.TenantId == tenantId
                  && session.SessionDate == reportDate
                  && detail.IsCrossLocation
                  && !detail.IsArchived
                  && !record.IsArchived
                  && !session.IsArchived
            select detail.Id
        ).CountAsync(cancellationToken);
    }
    public async Task<(
    IReadOnlyList<StudentsByBeltItem> Items,
    int TotalStudents)>
    GetStudentsByBeltAsync(
        Guid tenantId,
        Guid? organizationId,
        CancellationToken cancellationToken = default)
{
    var studentsQuery = _context.Set<Student>()
        .AsNoTracking()
        .Where(x =>
            x.TenantId == tenantId &&
            x.Status == StudentStatus.Active &&
            !x.IsArchived);

    if (organizationId.HasValue)
    {
        studentsQuery = studentsQuery.Where(
            x => x.OrganizationId == organizationId.Value);
    }

    var totalStudents = await studentsQuery.CountAsync(
        cancellationToken);

    var assignedStudents =
        from student in studentsQuery
        join beltRank in _context.Set<BeltRank>().AsNoTracking()
            on student.CurrentBeltRankId equals beltRank.Id
        where beltRank.IsActive &&
              !beltRank.IsArchived
        group student by new
        {
            BeltRankId = beltRank.Id,
            beltRank.BeltCode,
            beltRank.BeltName,
            beltRank.Level
        }
        into beltGroup
        select new
        {
            beltGroup.Key.BeltRankId,
            beltGroup.Key.BeltCode,
            beltGroup.Key.BeltName,
            beltGroup.Key.Level,
            StudentCount = beltGroup.Count()
        };

    var assignedItems = await assignedStudents
        .OrderBy(x => x.Level)
        .ToListAsync(cancellationToken);

    var items = assignedItems
        .Select(x => new StudentsByBeltItem(
            x.BeltRankId,
            x.BeltCode,
            x.BeltName,
            x.StudentCount,
            totalStudents == 0
                ? 0m
                : Math.Round(
                    x.StudentCount * 100m / totalStudents,
                    2)))
        .ToList();

    var unassignedCount = await studentsQuery.CountAsync(
        x => !x.CurrentBeltRankId.HasValue,
        cancellationToken);

    if (unassignedCount > 0)
    {
        items.Add(
            new StudentsByBeltItem(
                null,
                "UNASSIGNED",
                "Chưa xếp đai",
                unassignedCount,
                totalStudents == 0
                    ? 0m
                    : Math.Round(
                        unassignedCount * 100m / totalStudents,
                        2)));
    }

    return (items, totalStudents);
}
}