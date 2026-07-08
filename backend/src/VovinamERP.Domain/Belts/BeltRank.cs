using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Belts;

public sealed class BeltRank : AggregateRoot
{
    public string BeltCode { get; private set; } = default!;
    public string BeltName { get; private set; } = default!;
    public int Level { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private BeltRank()
    {
    }

    private BeltRank(
        string beltCode,
        string beltName,
        int level,
        string? description)
    {
        BeltCode = beltCode.Trim();
        BeltName = beltName.Trim();
        Level = level;
        Description = description?.Trim();
        IsActive = true;

        RaiseDomainEvent(new BeltRankCreatedEvent(Id, BeltCode, BeltName));
    }

    public static Result<BeltRank> Create(
        string beltCode,
        string beltName,
        int level,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(beltCode))
            return Result<BeltRank>.Failure(BeltRankErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(beltName))
            return Result<BeltRank>.Failure(BeltRankErrors.NameRequired);

        if (level <= 0)
            return Result<BeltRank>.Failure(BeltRankErrors.InvalidLevel);

        var beltRank = new BeltRank(beltCode, beltName, level, description);

        return Result<BeltRank>.Success(beltRank);
    }

    public Result Update(
        string beltName,
        int level,
        string? description)
    {
        if (IsArchived)
            return Result.Failure(BeltRankErrors.AlreadyArchived);

        if (string.IsNullOrWhiteSpace(beltName))
            return Result.Failure(BeltRankErrors.NameRequired);

        if (level <= 0)
            return Result.Failure(BeltRankErrors.InvalidLevel);

        BeltName = beltName.Trim();
        Level = level;
        Description = description?.Trim();

        RaiseDomainEvent(new BeltRankUpdatedEvent(Id));

        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        RaiseDomainEvent(new BeltRankDeactivatedEvent(Id));
    }

    public void Activate()
    {
        IsActive = true;
        RaiseDomainEvent(new BeltRankActivatedEvent(Id));
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        IsActive = false;
        base.Archive(userId);

        RaiseDomainEvent(new BeltRankArchivedEvent(Id));
    }
}