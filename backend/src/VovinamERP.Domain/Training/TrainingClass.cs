using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class TrainingClass : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public TrainingClassStatus Status { get; private set; }

    private TrainingClass() { }

    private TrainingClass(
        Guid tenantId,
        Guid organizationId,
        string code,
        string name,
        string? description)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
        Code = code.Trim();
        Name = name.Trim();
        Description = description?.Trim();
        Status = TrainingClassStatus.Active;

        RaiseDomainEvent(new TrainingClassCreatedEvent(Id, TenantId, Code, Name));
    }

    public static Result<TrainingClass> Create(
        Guid tenantId,
        Guid organizationId,
        string code,
        string name,
        string? description)
    {
        if (tenantId == Guid.Empty)
            return Result<TrainingClass>.Failure(TrainingErrors.TenantRequired);

        if (organizationId == Guid.Empty)
            return Result<TrainingClass>.Failure(TrainingErrors.OrganizationRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result<TrainingClass>.Failure(TrainingErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<TrainingClass>.Failure(TrainingErrors.NameRequired);

        return Result<TrainingClass>.Success(new TrainingClass(tenantId, organizationId, code, name, description));
    }

    public Result Rename(string name, string? description)
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TrainingErrors.NameRequired);

        Name = name.Trim();
        Description = description?.Trim();
        MarkUpdated(null);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = TrainingClassStatus.Archived;
        base.Archive(userId);
    }
}
