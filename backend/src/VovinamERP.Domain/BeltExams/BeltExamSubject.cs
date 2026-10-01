using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.BeltExams;

public sealed class BeltExamSubject : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid BeltExamId { get; private set; }

    public string Name { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    public decimal? MaximumScore { get; private set; }

    private BeltExamSubject()
    {
    }

    private BeltExamSubject(
        Guid tenantId,
        Guid beltExamId,
        string name,
        int displayOrder,
        decimal? maximumScore,
        Guid? userId)
    {
        TenantId = tenantId;
        BeltExamId = beltExamId;
        Name = name.Trim();
        DisplayOrder = displayOrder;
        MaximumScore = maximumScore;
        CreatedBy = userId;
    }

    public static Result<BeltExamSubject> Create(
        Guid tenantId,
        Guid beltExamId,
        string name,
        int displayOrder,
        decimal? maximumScore,
        Guid? userId)
    {
        if (tenantId == Guid.Empty)
            return Result<BeltExamSubject>.Failure(
                BeltExamSubjectErrors.TenantRequired);

        if (beltExamId == Guid.Empty)
            return Result<BeltExamSubject>.Failure(
                BeltExamSubjectErrors.BeltExamRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<BeltExamSubject>.Failure(
                BeltExamSubjectErrors.NameRequired);

        if (displayOrder <= 0)
            return Result<BeltExamSubject>.Failure(
                BeltExamSubjectErrors.InvalidDisplayOrder);

        if (maximumScore.HasValue && maximumScore.Value <= 0)
            return Result<BeltExamSubject>.Failure(
                BeltExamSubjectErrors.InvalidMaximumScore);

        return Result<BeltExamSubject>.Success(
            new BeltExamSubject(
                tenantId,
                beltExamId,
                name,
                displayOrder,
                maximumScore,
                userId));
    }
}