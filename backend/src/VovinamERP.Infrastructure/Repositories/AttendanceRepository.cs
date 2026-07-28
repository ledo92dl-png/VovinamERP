using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Training;
using VovinamERP.Infrastructure.Persistence;
using VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class AttendanceRepository : IAttendanceRepository
{
   public async Task<(int TotalAttendances, int CrossLocationAttendances)>
    GetCrossLocationSummaryAsync(
        Guid tenantId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
{
    var query =
        from detail in _context.Set<AttendanceDetail>()
            .AsNoTracking()
        join record in _context.Set<AttendanceRecord>()
            .AsNoTracking()
            on detail.AttendanceRecordId equals record.Id
        join session in _context.Set<TrainingSession>()
            .AsNoTracking()
            on record.TrainingSessionId equals session.Id
        where detail.TenantId == tenantId
              && record.TenantId == tenantId
              && session.TenantId == tenantId
              && !detail.IsArchived
              && !record.IsArchived
              && !session.IsArchived
        select new
        {
            detail.IsCrossLocation,
            session.SessionDate
        };

    if (fromDate.HasValue)
    {
        query = query.Where(
            x => x.SessionDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        query = query.Where(
            x => x.SessionDate <= toDate.Value);
    }

    var totalAttendances =
        await query.CountAsync(cancellationToken);

    var crossLocationAttendances =
        await query.CountAsync(
            x => x.IsCrossLocation,
            cancellationToken);

    return (
        totalAttendances,
        crossLocationAttendances);
}
    public async Task<bool> ExistsForTrainingSessionAsync(
    Guid tenantId,
    Guid trainingSessionId,
    CancellationToken cancellationToken = default)
{
    return await _context.Set<AttendanceRecord>()
        .AsNoTracking()
        .AnyAsync(
            x => x.TenantId == tenantId &&
                 x.TrainingSessionId == trainingSessionId,
            cancellationToken);
}
    private readonly VovinamDbContext _context;

    public async Task<IReadOnlyList<AttendanceRecord>> GetPagedAsync(
    Guid tenantId,
    Guid? trainingSessionId,
    int skip,
    int take,
    CancellationToken cancellationToken = default)
{
    var query = _context.Set<AttendanceRecord>()
        .AsNoTracking()
        .Include(x => x.Details)
        .Where(x => x.TenantId == tenantId);

    if (trainingSessionId.HasValue)
    {
        query = query.Where(
            x => x.TrainingSessionId == trainingSessionId.Value);
    }

    return await query
    .OrderByDescending(x => x.Id)
    .Skip(skip)
    .Take(take)
    .ToListAsync(cancellationToken);
}

public async Task<int> CountAsync(
    Guid tenantId,
    Guid? trainingSessionId,
    CancellationToken cancellationToken = default)
{
    var query = _context.Set<AttendanceRecord>()
        .AsNoTracking()
        .Where(x => x.TenantId == tenantId);

    if (trainingSessionId.HasValue)
    {
        query = query.Where(
            x => x.TrainingSessionId == trainingSessionId.Value);
    }

    return await query.CountAsync(cancellationToken);
}
public AttendanceRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceRecord?> GetRecordByIdAsync(
        Guid attendanceRecordId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AttendanceRecord>()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(
                x => x.Id == attendanceRecordId,
                cancellationToken);
    }

    public async Task<AttendanceRecord?> GetByIdAsync(
        Guid attendanceRecordId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AttendanceRecord>()
            .AsNoTracking()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(
                x => x.Id == attendanceRecordId &&
                     x.TenantId == tenantId,
                cancellationToken);
    }

    public async Task AddRecordAsync(
        AttendanceRecord attendanceRecord,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<AttendanceRecord>()
            .AddAsync(attendanceRecord, cancellationToken);
    }

    public void UpdateRecord(AttendanceRecord attendanceRecord)
    {
        _context.Set<AttendanceRecord>()
            .Update(attendanceRecord);
    }
    public async Task<IReadOnlyList<CrossLocationOrganizationItem>>
    GetCrossLocationByOrganizationAsync(
        Guid tenantId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
{
    var query =
        from detail in _context.Set<AttendanceDetail>().AsNoTracking()
        join record in _context.Set<AttendanceRecord>().AsNoTracking()
            on detail.AttendanceRecordId equals record.Id
        join session in _context.Set<TrainingSession>().AsNoTracking()
            on record.TrainingSessionId equals session.Id
        join trainingClass in _context.Set<TrainingClass>().AsNoTracking()
            on session.TrainingClassId equals trainingClass.Id
        join organization in _context.Set<Organization>().AsNoTracking()
            on trainingClass.OrganizationId equals organization.Id
        where detail.TenantId == tenantId
              && !detail.IsArchived
              && !record.IsArchived
              && !session.IsArchived
        select new
        {
            organization.Id,
            organization.Name,
            detail.IsCrossLocation,
            session.SessionDate
        };

    if (fromDate.HasValue)
        query = query.Where(x => x.SessionDate >= fromDate.Value);

    if (toDate.HasValue)
        query = query.Where(x => x.SessionDate <= toDate.Value);

    var data = await query.ToListAsync(cancellationToken);

    return data
        .GroupBy(x => new { x.Id, x.Name })
        .Select(g =>
        {
            var total = g.Count();
            var cross = g.Count(x => x.IsCrossLocation);

            return new CrossLocationOrganizationItem(
                g.Key.Id,
                g.Key.Name,
                total,
                cross,
                total == 0
                    ? 0m
                    : Math.Round(cross * 100m / total, 2));
        })
        .OrderByDescending(x => x.CrossLocationAttendances)
        .ToList();
}
}