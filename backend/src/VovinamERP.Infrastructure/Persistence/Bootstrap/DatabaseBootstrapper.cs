using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Domain.Tenants;
using VovinamERP.Infrastructure.Identity;
using VovinamERP.Domain.Organizations;

namespace VovinamERP.Infrastructure.Persistence.Bootstrap;

public static class DatabaseBootstrapper
{
    private static readonly string[] DefaultRoles =
    [
        "SuperAdmin",
        "Admin",
        "Manager",
        "Coach",
        "Accountant",
        "Student",
        "Parent"
    ];

    public static async Task BootstrapAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var services = scope.ServiceProvider;

        var dbContext =
            services.GetRequiredService<VovinamDbContext>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var configuration =
            services.GetRequiredService<IConfiguration>();

        // -------------------------------------------------
        // 1. TENANT
        // -------------------------------------------------

        const string tenantCode = "VTP";

        var tenant = await dbContext.Tenants
            .FirstOrDefaultAsync(
                x => x.Code == tenantCode,
                cancellationToken);

        if (tenant is null)
        {
            var tenantResult = Tenant.Create(
                tenantCode,
                "Vovinam Tam Phước");

            if (tenantResult.IsFailure ||
                tenantResult.Value is null)
            {
                throw new InvalidOperationException(
                    tenantResult.Error.Message);
            }

            tenant = tenantResult.Value;

            await dbContext.Tenants.AddAsync(
                tenant,
                cancellationToken);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        // -------------------------------------------------
// 2. ORGANIZATION
// -------------------------------------------------

const string organizationCode = "CLB-TP";

var organization = await dbContext.Organizations
    .FirstOrDefaultAsync(
        x => x.TenantId == tenant.Id &&
             x.Code == organizationCode,
        cancellationToken);

if (organization is null)
{
    var organizationResult = Organization.Create(
        tenant.Id,
        null,
        organizationCode,
        "Vovinam Tam Phước",
        OrganizationType.Club,
        null,
        null,
        null);

    if (organizationResult.IsFailure ||
        organizationResult.Value is null)
    {
        throw new InvalidOperationException(
            organizationResult.Error.Message);
    }

    organization = organizationResult.Value;

    await dbContext.Organizations.AddAsync(
        organization,
        cancellationToken);

    await dbContext.SaveChangesAsync(
        cancellationToken);
}

        // -------------------------------------------------
        // 3. ROLES
        // -------------------------------------------------

        foreach (var roleName in DefaultRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                });

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    BuildIdentityErrorMessage(
                        $"Could not create role '{roleName}'.",
                        roleResult.Errors));
            }
        }

        // -------------------------------------------------
        // 4. INITIAL SUPER ADMIN
        // -------------------------------------------------

        var adminUserName =
            configuration["Bootstrap:AdminUserName"];

        var adminEmail =
            configuration["Bootstrap:AdminEmail"];

        var adminPassword =
            configuration["Bootstrap:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            // Không tạo Admin nếu chưa cấu hình secret.
            return;
        }

        var adminUser =
            await userManager.FindByNameAsync(adminUserName);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserName = adminUserName.Trim(),
                Email = string.IsNullOrWhiteSpace(adminEmail)
                    ? null
                    : adminEmail.Trim(),
                IsActive = true
            };

            var createUserResult =
                await userManager.CreateAsync(
                    adminUser,
                    adminPassword);

            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    BuildIdentityErrorMessage(
                        "Could not create initial SuperAdmin user.",
                        createUserResult.Errors));
            }
        }

        // Nếu user đã tồn tại nhưng TenantId chưa đúng,
        // đồng bộ lại cho môi trường bootstrap.
        if (adminUser.TenantId != tenant.Id)
        {
            adminUser.TenantId = tenant.Id;

            var updateResult =
                await userManager.UpdateAsync(adminUser);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    BuildIdentityErrorMessage(
                        "Could not update SuperAdmin tenant.",
                        updateResult.Errors));
            }
        }

        if (!await userManager.IsInRoleAsync(
            adminUser,
            "SuperAdmin"))
        {
            var addRoleResult =
                await userManager.AddToRoleAsync(
                    adminUser,
                    "SuperAdmin");

            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    BuildIdentityErrorMessage(
                        "Could not assign SuperAdmin role.",
                        addRoleResult.Errors));
            }
        }
    }

    private static string BuildIdentityErrorMessage(
        string prefix,
        IEnumerable<IdentityError> errors)
    {
        var details = string.Join(
            "; ",
            errors.Select(
                x => $"{x.Code}: {x.Description}"));

        return string.IsNullOrWhiteSpace(details)
            ? prefix
            : $"{prefix} {details}";
    }
}