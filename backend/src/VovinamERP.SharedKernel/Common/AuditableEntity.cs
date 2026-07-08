namespace VovinamERP.SharedKernel.Common;

public abstract class AuditableEntity : EntityBase
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    public bool IsArchived { get; protected set; }
    public DateTime? ArchivedAtUtc { get; protected set; }
    public Guid? ArchivedBy { get; protected set; }

    public virtual void MarkUpdated(Guid? userId)
    {
        UpdatedBy = userId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public virtual void Archive(Guid? userId)
    {
        if (IsArchived) return;

        IsArchived = true;
        ArchivedBy = userId;
        ArchivedAtUtc = DateTime.UtcNow;
    }
}
