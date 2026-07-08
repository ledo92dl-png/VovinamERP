using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Organizations;

public sealed class Organization : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid? ParentId { get; private set; }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public OrganizationType OrganizationType { get; private set; }

    public string? Address { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }

    public OrganizationStatus Status { get; private set; }

    private Organization() { }

    private Organization(
        Guid tenantId,
        Guid? parentId,
        string code,
        string name,
        OrganizationType organizationType,
        string? address,
        string? phoneNumber,
        string? email)
    {
        TenantId = tenantId;
        ParentId = parentId;
        Code = code.Trim();
        Name = name.Trim();
        OrganizationType = organizationType;
        Address = address?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        Status = OrganizationStatus.Active;
    }

    public static Result<Organization> Create(
        Guid tenantId,
        Guid? parentId,
        string code,
        string name,
        OrganizationType organizationType,
        string? address,
        string? phoneNumber,
        string? email)
    {
        if (tenantId == Guid.Empty)
            return Result<Organization>.Failure(new Error("ORG_001", "Tenant is required."));

        if (string.IsNullOrWhiteSpace(code))
            return Result<Organization>.Failure(new Error("ORG_002", "Organization code is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result<Organization>.Failure(new Error("ORG_003", "Organization name is required."));

        return Result<Organization>.Success(
            new Organization(tenantId, parentId, code, name, organizationType, address, phoneNumber, email));
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = OrganizationStatus.Archived;
        base.Archive(userId);
    }
}