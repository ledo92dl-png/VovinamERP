using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;
using VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;
using VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Training;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;


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
    public async Task<(
    IReadOnlyList<CrossLocationAttendanceDetailItem> Items,
    int TotalCount)>
    GetCrossLocationAttendanceDetailsAsync(
        Guid tenantId,
        DateOnly? fromDate,
        DateOnly? toDate,
        Guid? studentId,
        Guid? trainingOrganizationId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool descending,
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
        join student in _context.Set<Student>().AsNoTracking()
            on detail.StudentId equals student.Id
        join person in _context.Set<Person>().AsNoTracking()
            on student.PersonId equals person.Id
        join homeOrganization in _context.Set<Organization>().AsNoTracking()
            on student.OrganizationId equals homeOrganization.Id
        join trainingOrganization in _context.Set<Organization>().AsNoTracking()
            on trainingClass.OrganizationId equals trainingOrganization.Id
        where detail.TenantId == tenantId
              && record.TenantId == tenantId
              && session.TenantId == tenantId
              && student.TenantId == tenantId
              && person.TenantId == tenantId
              && homeOrganization.TenantId == tenantId
              && trainingOrganization.TenantId == tenantId
              && detail.IsCrossLocation
              && !detail.IsArchived
              && !record.IsArchived
              && !session.IsArchived
              && !student.IsArchived
              && !person.IsArchived
              && !homeOrganization.IsArchived
              && !trainingOrganization.IsArchived
        select new
        {
            Detail = detail,
            Session = session,
            TrainingClass = trainingClass,
            Student = student,
            Person = person,
            HomeOrganization = homeOrganization,
            TrainingOrganization = trainingOrganization
        };

    if (fromDate.HasValue)
    {
        query = query.Where(
            x => x.Session.SessionDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        query = query.Where(
            x => x.Session.SessionDate <= toDate.Value);
    }

    if (studentId.HasValue)
    {
        query = query.Where(
            x => x.Student.Id == studentId.Value);
    }

    if (trainingOrganizationId.HasValue)
    {
        query = query.Where(
            x => x.TrainingOrganization.Id ==
                 trainingOrganizationId.Value);
    }

    var totalCount = await query.CountAsync(
        cancellationToken);

    var normalizedSortBy =
        sortBy?.Trim().ToLowerInvariant() ?? "sessiondate";

    query = (normalizedSortBy, descending) switch
    {
        ("markedat", true) =>
            query.OrderByDescending(x => x.Detail.MarkedAt),

        ("markedat", false) =>
            query.OrderBy(x => x.Detail.MarkedAt),

        ("fullname", true) =>
            query.OrderByDescending(x => x.Person.FullName),

        ("fullname", false) =>
            query.OrderBy(x => x.Person.FullName),

        ("membernumber", true) =>
            query.OrderByDescending(x => x.Student.MemberNumber),

        ("membernumber", false) =>
            query.OrderBy(x => x.Student.MemberNumber),

        ("trainingorganizationname", true) =>
            query.OrderByDescending(
                x => x.TrainingOrganization.Name),

        ("trainingorganizationname", false) =>
            query.OrderBy(
                x => x.TrainingOrganization.Name),

        ("sessiondate", false) =>
            query.OrderBy(x => x.Session.SessionDate)
                .ThenBy(x => x.Session.StartTime),

        _ =>
            query.OrderByDescending(x => x.Session.SessionDate)
                .ThenByDescending(x => x.Session.StartTime)
    };

    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new CrossLocationAttendanceDetailItem(
            x.Detail.Id,
            x.Student.Id,
            x.Student.MemberNumber,
            x.Person.FullName,

            x.HomeOrganization.Id,
            x.HomeOrganization.Name,

            x.TrainingOrganization.Id,
            x.TrainingOrganization.Name,

            x.TrainingClass.Id,
            x.TrainingClass.Code,
            x.TrainingClass.Name,

            x.Session.Id,
            x.Session.SessionDate,
            x.Session.StartTime,
            x.Session.EndTime,

            x.Detail.Status,
            x.Detail.Method,
            x.Detail.MarkedAt,
            x.Detail.Note))
        .ToListAsync(cancellationToken);

    return (items, totalCount);
}
    public async Task<IReadOnlyList<CrossLocationStudentItem>>
    GetCrossLocationByStudentAsync(
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
        join student in _context.Set<Student>()
            .AsNoTracking()
            on detail.StudentId equals student.Id
        join person in _context.Set<Person>()
            .AsNoTracking()
            on student.PersonId equals person.Id
        join homeOrganization in _context.Set<Organization>()
            .AsNoTracking()
            on student.OrganizationId equals homeOrganization.Id
        where detail.TenantId == tenantId
              && record.TenantId == tenantId
              && session.TenantId == tenantId
              && student.TenantId == tenantId
              && person.TenantId == tenantId
              && homeOrganization.TenantId == tenantId
              && !detail.IsArchived
              && !record.IsArchived
              && !session.IsArchived
              && !student.IsArchived
              && !person.IsArchived
              && !homeOrganization.IsArchived
        select new
        {
            StudentId = student.Id,
            student.MemberNumber,
            person.FullName,
            HomeOrganizationName = homeOrganization.Name,
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

    var data = await query.ToListAsync(
        cancellationToken);

    return data
        .GroupBy(x => new
        {
            x.StudentId,
            x.MemberNumber,
            x.FullName,
            x.HomeOrganizationName
        })
        .Select(group =>
        {
            var totalAttendances = group.Count();

            var crossLocationAttendances =
                group.Count(x => x.IsCrossLocation);

            var crossLocationRate =
                totalAttendances == 0
                    ? 0m
                    : Math.Round(
                        crossLocationAttendances * 100m /
                        totalAttendances,
                        2);

            return new CrossLocationStudentItem(
                group.Key.StudentId,
                group.Key.MemberNumber,
                group.Key.FullName,
                group.Key.HomeOrganizationName,
                totalAttendances,
                crossLocationAttendances,
                crossLocationRate);
        })
        .OrderByDescending(
            x => x.CrossLocationAttendances)
        .ThenBy(x => x.FullName)
        .ToList();
}
    public async Task<IReadOnlyList<CrossLocationAttendanceDetailItem>>
    GetCrossLocationAttendanceDetailsAsync(
        Guid tenantId,
        DateOnly? fromDate,
        DateOnly? toDate,
        Guid? studentId,
        Guid? trainingOrganizationId,
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
        join student in _context.Set<Student>().AsNoTracking()
            on detail.StudentId equals student.Id
        join person in _context.Set<Person>().AsNoTracking()
            on student.PersonId equals person.Id
        join homeOrganization in _context.Set<Organization>().AsNoTracking()
            on student.OrganizationId equals homeOrganization.Id
        join trainingOrganization in _context.Set<Organization>().AsNoTracking()
            on trainingClass.OrganizationId equals trainingOrganization.Id
        where detail.TenantId == tenantId
              && detail.IsCrossLocation
              && !detail.IsArchived
        select new
        {
            Detail = detail,
            Session = session,
            TrainingClass = trainingClass,
            Student = student,
            Person = person,
            HomeOrganization = homeOrganization,
            TrainingOrganization = trainingOrganization
        };

    if (fromDate.HasValue)
        query = query.Where(x => x.Session.SessionDate >= fromDate.Value);

    if (toDate.HasValue)
        query = query.Where(x => x.Session.SessionDate <= toDate.Value);

    if (studentId.HasValue)
        query = query.Where(x => x.Student.Id == studentId.Value);

    if (trainingOrganizationId.HasValue)
        query = query.Where(
            x => x.TrainingOrganization.Id == trainingOrganizationId.Value);

    return await query
        .OrderByDescending(x => x.Session.SessionDate)
        .ThenByDescending(x => x.Detail.MarkedAt)
        .Select(x => new CrossLocationAttendanceDetailItem(
            x.Detail.Id,
            x.Student.Id,
            x.Student.MemberNumber,
            x.Person.FullName,

            x.HomeOrganization.Id,
            x.HomeOrganization.Name,

            x.TrainingOrganization.Id,
            x.TrainingOrganization.Name,

            x.TrainingClass.Id,
            x.TrainingClass.Code,
            x.TrainingClass.Name,

            x.Session.Id,
            x.Session.SessionDate,
            x.Session.StartTime,
            x.Session.EndTime,

            x.Detail.Status,
            x.Detail.Method,
            x.Detail.MarkedAt,
            x.Detail.Note))
        .ToListAsync(cancellationToken);
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
              && detail.IsCrossLocation
              && !detail.IsArchived
              && !record.IsArchived
              && !session.IsArchived
              && !trainingClass.IsArchived
              && !organization.IsArchived
        select new
        {
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            SessionDate = session.SessionDate
        };

    if (fromDate.HasValue)
        query = query.Where(x => x.SessionDate >= fromDate.Value);

    if (toDate.HasValue)
        query = query.Where(x => x.SessionDate <= toDate.Value);

    var data = await query.ToListAsync(cancellationToken);

    return data
        .GroupBy(x => new
        {
            x.OrganizationId,
            x.OrganizationName
        })
        .Select(g =>
{
    var totalAttendances = g.Count();

    var crossLocationAttendances =
        g.Count();

    var crossLocationRate =
        totalAttendances == 0
            ? 0m
            : Math.Round(
                crossLocationAttendances * 100m /
                totalAttendances,
                2);

    return new CrossLocationOrganizationItem(
        g.Key.OrganizationId,
        g.Key.OrganizationName,
        totalAttendances,
        crossLocationAttendances,
        crossLocationRate);
})
        .OrderByDescending(x => x.CrossLocationAttendances)
        .ThenBy(x => x.OrganizationName)
        .ToList();
}
}