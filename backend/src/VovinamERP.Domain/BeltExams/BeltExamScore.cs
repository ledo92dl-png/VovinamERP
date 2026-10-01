using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public sealed class BeltExamScore : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid BeltExamStudentResultId { get; private set; }
    public Guid BeltExamSubjectId { get; private set; }

    public decimal Score { get; private set; }

    private BeltExamScore()
    {
    }

    private BeltExamScore(
        Guid tenantId,
        Guid beltExamStudentResultId,
        Guid beltExamSubjectId,
        decimal score,
        Guid? userId)
    {
        TenantId = tenantId;
        BeltExamStudentResultId = beltExamStudentResultId;
        BeltExamSubjectId = beltExamSubjectId;
        Score = score;
        CreatedBy = userId;
    }

    public static Result<BeltExamScore> Create(
        Guid tenantId,
        Guid beltExamStudentResultId,
        Guid beltExamSubjectId,
        decimal score,
        Guid? userId)
    {
        if (tenantId == Guid.Empty)
            return Result<BeltExamScore>.Failure(
                BeltExamScoreErrors.TenantRequired);

        if (beltExamStudentResultId == Guid.Empty)
            return Result<BeltExamScore>.Failure(
                BeltExamScoreErrors.StudentResultRequired);

        if (beltExamSubjectId == Guid.Empty)
            return Result<BeltExamScore>.Failure(
                BeltExamScoreErrors.SubjectRequired);

        if (score < 0)
            return Result<BeltExamScore>.Failure(
                BeltExamScoreErrors.NegativeScore);

        return Result<BeltExamScore>.Success(
            new BeltExamScore(
                tenantId,
                beltExamStudentResultId,
                beltExamSubjectId,
                score,
                userId));
    }
}