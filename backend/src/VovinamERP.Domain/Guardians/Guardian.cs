using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Guardians;

public sealed class Guardian : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PersonId { get; private set; }
    public string? RelationshipNote { get; private set; }

    private Guardian() { }

    private Guardian(Guid tenantId, Guid personId, string? relationshipNote)
    {
        TenantId = tenantId;
        PersonId = personId;
        RelationshipNote = relationshipNote?.Trim();
    }

    public static Result<Guardian> Create(
        Guid tenantId,
        Guid personId,
        string? relationshipNote)
    {
        if (tenantId == Guid.Empty)
            return Result<Guardian>.Failure(GuardianErrors.TenantRequired);

        if (personId == Guid.Empty)
            return Result<Guardian>.Failure(GuardianErrors.PersonRequired);

        return Result<Guardian>.Success(new Guardian(tenantId, personId, relationshipNote));
    }

    public Result UpdateRelationshipNote(string? relationshipNote, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(GuardianErrors.AlreadyArchived);

        RelationshipNote = relationshipNote?.Trim();
        MarkUpdated(userId);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        base.Archive(userId);
    }
}
