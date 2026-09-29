using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Tenants;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/app-context")]
public sealed class AppContextController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public AppContextController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<AppContextResponse>> Get(
        CancellationToken cancellationToken)
    {
        const string tenantCode = "VTP";
        const string organizationCode = "CLB-TP";

        var tenant = await _dbContext.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Code == tenantCode,
                cancellationToken);

        if (tenant is null)
        {
            return NotFound(
                "Default tenant was not found.");
        }

        var organization = await _dbContext.Set<Organization>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenant.Id &&
                     x.Code == organizationCode,
                cancellationToken);

        if (organization is null)
        {
            return NotFound(
                "Default organization was not found.");
        }

        return Ok(new AppContextResponse(
            tenant.Id,
            tenant.Code,
            tenant.Name,
            organization.Id,
            organization.Code,
            organization.Name));
    }
}

public sealed record AppContextResponse(
    Guid TenantId,
    string TenantCode,
    string TenantName,
    Guid OrganizationId,
    string OrganizationCode,
    string OrganizationName);