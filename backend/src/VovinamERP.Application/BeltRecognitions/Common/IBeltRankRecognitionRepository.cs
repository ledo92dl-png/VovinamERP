namespace VovinamERP.Application.BeltRecognitions.Common;

public interface IBeltRankRecognitionRepository
{
    Task<IReadOnlyList<BeltRankRecognitionListItem>> ListByStudentAsync(
        Guid tenantId,
        Guid studentId,
        CancellationToken cancellationToken = default);
}