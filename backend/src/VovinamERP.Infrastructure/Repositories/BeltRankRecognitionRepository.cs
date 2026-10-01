using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.BeltRecognitions.Common;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Belts;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class BeltRankRecognitionRepository
    : IBeltRankRecognitionRepository
{
    private readonly VovinamDbContext _context;

    public BeltRankRecognitionRepository(
        VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BeltRankRecognitionListItem>> ListByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from recognition in _context
                .Set<BeltRankRecognition>()
                .AsNoTracking()
            join belt in _context
                .Set<BeltRank>()
                .AsNoTracking()
                on recognition.BeltRankId equals belt.Id
            join document in _context
                .Set<BeltRankDocument>()
                .AsNoTracking()
                on new
                {
                    recognition.TenantId,
                    BeltRankRecognitionId = recognition.Id
                }
                equals new
                {
                    document.TenantId,
                    document.BeltRankRecognitionId
                }
                into documentGroup
            from document in documentGroup.DefaultIfEmpty()
            where recognition.TenantId == tenantId
                  && recognition.StudentId == studentId
                  && !recognition.IsArchived
            orderby recognition.RecognitionDate descending,
                recognition.CreatedAtUtc descending
            select new BeltRankRecognitionListItem(
                recognition.Id,
                recognition.BeltRankId,
                belt.BeltCode,
                belt.BeltName,
                belt.Level,
                recognition.RecognitionDate,
                recognition.Source,
                recognition.BeltExamStudentResultId,
                recognition.Note,
                document == null
                    ? null
                    : new BeltRankDocumentListItem(
                        document.Id,
                        document.DocumentType,
                        document.DocumentNumber,
                        document.SignedDate,
                        document.ScanUrl,
                        document.Note)))
            .ToListAsync(cancellationToken);
    }
}