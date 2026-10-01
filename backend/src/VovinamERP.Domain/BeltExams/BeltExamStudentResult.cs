using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public sealed class BeltExamStudentResult : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid BeltExamId { get; private set; }
    public Guid StudentId { get; private set; }

    public string UnitName { get; private set; } = default!;
    public string SourceResult { get; private set; } = default!;
    public StudentBeltResult Result { get; private set; }

    public decimal? TotalScore { get; private set; }
    public int? Ranking { get; private set; }
    public string? Note { get; private set; }

    private BeltExamStudentResult()
    {
    }

    private BeltExamStudentResult(
        Guid tenantId,
        Guid beltExamId,
        Guid studentId,
        string unitName,
        string sourceResult,
        StudentBeltResult result,
        decimal? totalScore,
        int? ranking,
        string? note,
        Guid? userId)
    {
        TenantId = tenantId;
        BeltExamId = beltExamId;
        StudentId = studentId;
        UnitName = unitName.Trim();
        SourceResult = sourceResult.Trim();
        Result = result;
        TotalScore = totalScore;
        Ranking = ranking;
        Note = note?.Trim();
        CreatedBy = userId;
    }

    public static Result<BeltExamStudentResult> Create(
        Guid tenantId,
        Guid beltExamId,
        Guid studentId,
        string unitName,
        string sourceResult,
        StudentBeltResult result,
        decimal? totalScore,
        int? ranking,
        string? note,
        Guid? userId)
    {
        if (tenantId == Guid.Empty)
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.TenantRequired);

        if (beltExamId == Guid.Empty)
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.BeltExamRequired);

        if (studentId == Guid.Empty)
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.StudentRequired);

        if (string.IsNullOrWhiteSpace(unitName))
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.UnitNameRequired);

        if (string.IsNullOrWhiteSpace(sourceResult))
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.SourceResultRequired);

        if (!Enum.IsDefined(result))
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.InvalidResult);

        if (totalScore.HasValue && totalScore.Value < 0)
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.NegativeTotalScore);

        if (ranking.HasValue && ranking.Value <= 0)
            return Result<BeltExamStudentResult>.Failure(
                BeltExamStudentResultErrors.InvalidRanking);

        return Result<BeltExamStudentResult>.Success(
            new BeltExamStudentResult(
                tenantId,
                beltExamId,
                studentId,
                unitName,
                sourceResult,
                result,
                totalScore,
                ranking,
                note,
                userId));
    }
}