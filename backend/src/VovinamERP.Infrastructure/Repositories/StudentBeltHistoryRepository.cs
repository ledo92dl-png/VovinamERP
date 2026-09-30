using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class StudentBeltHistoryRepository
    : IStudentBeltHistoryRepository
{
    private readonly VovinamDbContext _context;

    public StudentBeltHistoryRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StudentBeltHistoryListItem>> ListByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from history in _context.Set<StudentBeltHistory>().AsNoTracking()
            join belt in _context.Set<BeltRank>().AsNoTracking()
                on history.BeltRankId equals belt.Id
            where history.TenantId == tenantId
                  && history.StudentId == studentId
                  && !history.IsArchived
            orderby history.ExamDate descending, history.CreatedAtUtc descending
            select new StudentBeltHistoryListItem(
                history.Id,
                history.BeltRankId,
                belt.BeltCode,
                belt.BeltName,
                belt.Level,
                history.ExamDate,
                history.Result,
                history.AwardedDate,
                history.Note))
            .ToListAsync(cancellationToken);
    }
}