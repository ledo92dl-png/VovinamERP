using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Domain.Tenants;
using VovinamERP.Infrastructure.Identity;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Belts;

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
                "Vovinam Tam PhÆ°á»›c");

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
        "Vovinam Tam PhÆ°á»›c",
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
        // 3. BELT RANKS
        // -------------------------------------------------

        var defaultBeltRanks = new[]
        {
            new { Code = "TVNM",       Name = "Tá»± vá»‡ nháº­p mÃ´n",       Level = 1 },
            new { Code = "LAM",        Name = "Lam Ä‘ai",              Level = 2 },
            new { Code = "LAM-1",      Name = "Lam Ä‘ai I",            Level = 3 },
            new { Code = "LAM-2",      Name = "Lam Ä‘ai II",           Level = 4 },
            new { Code = "LAM-3",      Name = "Lam Ä‘ai III",          Level = 5 },

            // Hoang dai thieu nhi and Hoang dai are the same rank level.
            new { Code = "HOANG-TN",   Name = "HoÃ ng Ä‘ai thiáº¿u nhi",  Level = 6 },
            new { Code = "HOANG",      Name = "HoÃ ng Ä‘ai",            Level = 6 },

            new { Code = "HOANG-1",    Name = "HoÃ ng Ä‘ai I",          Level = 7 },
            new { Code = "HOANG-2",    Name = "HoÃ ng Ä‘ai II",         Level = 8 },
            new { Code = "HOANG-3",    Name = "HoÃ ng Ä‘ai III",        Level = 9 },
            new { Code = "CHUAN-HONG", Name = "Chuáº©n Há»“ng Ä‘ai",       Level = 10 },
            new { Code = "HONG",       Name = "Há»“ng Ä‘ai",             Level = 11 },
            new { Code = "HONG-1",     Name = "Há»“ng Ä‘ai I",           Level = 12 },
            new { Code = "HONG-2",     Name = "Há»“ng Ä‘ai II",          Level = 13 },
            new { Code = "HONG-3",     Name = "Há»“ng Ä‘ai III",         Level = 14 }
        };

        foreach (var item in defaultBeltRanks)
        {
            var exists = await dbContext.BeltRanks
                .AnyAsync(
                    x => x.BeltCode == item.Code,
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            var beltRankResult = BeltRank.Create(
                item.Code,
                item.Name,
                item.Level,
                null);

            if (beltRankResult.IsFailure ||
                beltRankResult.Value is null)
            {
                throw new InvalidOperationException(
                    beltRankResult.Error.Message);
            }

            await dbContext.BeltRanks.AddAsync(
                beltRankResult.Value,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // -------------------------------------------------
        // 4. ROLES
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
        // 5. INITIAL SUPER ADMIN
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
            // KhÃ´ng táº¡o Admin náº¿u chÆ°a cáº¥u hÃ¬nh secret.
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

        // Náº¿u user Ä‘Ã£ tá»“n táº¡i nhÆ°ng TenantId chÆ°a Ä‘Ãºng,
        // Ä‘á»“ng bá»™ láº¡i cho mÃ´i trÆ°á»ng bootstrap.
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
