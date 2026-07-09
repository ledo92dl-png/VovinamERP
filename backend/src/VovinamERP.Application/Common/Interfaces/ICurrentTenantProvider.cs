namespace VovinamERP.Application.Common.Interfaces;

public interface ICurrentTenantProvider
{
    Guid? TenantId { get; }
}
