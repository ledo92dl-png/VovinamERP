using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Tenants;

public sealed class Tenant : AggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public TenantStatus Status { get; private set; }

    private Tenant() { }

    private Tenant(string code, string name)
    {
        Code = code.Trim();
        Name = name.Trim();
        Status = TenantStatus.Active;
    }

    public static Result<Tenant> Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result<Tenant>.Failure(new Error("TENANT_001", "Tenant code is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result<Tenant>.Failure(new Error("TENANT_002", "Tenant name is required."));

        return Result<Tenant>.Success(new Tenant(code, name));
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("TENANT_002", "Tenant name is required."));

        Name = name.Trim();
        MarkUpdated(null);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = TenantStatus.Archived;
        base.Archive(userId);
    }
}