using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Infrastructure.Identity;

namespace VovinamERP.Tests;

public sealed class LoginLockoutIntegrationTests
{
    [Fact]
    public async Task FiveWrongPasswords_ShouldLockAccount()
    {
        await using var db = TestDatabase.CreateContext();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDataProtection();
        services.AddAuthentication();

        services.AddSingleton<
            Microsoft.AspNetCore.Authentication.ISystemClock>(
            new Microsoft.AspNetCore.Authentication.SystemClock());

        services.AddSingleton<
            Microsoft.AspNetCore.Http.IHttpContextAccessor,
            Microsoft.AspNetCore.Http.HttpContextAccessor>();

        services.AddSingleton(db);

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
        .AddSignInManager()
        .AddDefaultTokenProviders();

        services.AddScoped<
            IUserStore<ApplicationUser>>(
            _ => new Microsoft.AspNetCore.Identity.EntityFrameworkCore
                .UserStore<ApplicationUser, IdentityRole<Guid>,
                    VovinamERP.Infrastructure.Persistence.VovinamDbContext,
                    Guid>(db));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var userManager =
            scope.ServiceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        var signInManager =
            scope.ServiceProvider.GetRequiredService<
                SignInManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            UserName = "lockout_test_" +
                Guid.NewGuid().ToString("N"),
            IsActive = true,
            LockoutEnabled = true
        };

        const string correctPassword = "TestPassword123";

        var createResult = await userManager.CreateAsync(
            user, correctPassword);

        Assert.True(createResult.Succeeded,
            string.Join("; ",
                createResult.Errors.Select(e => e.Description)));

        try
        {
            for (var i = 1; i <= 5; i++)
            {
                var result =
                    await signInManager.CheckPasswordSignInAsync(
                        user,
                        "WrongPassword123",
                        lockoutOnFailure: true);

                Assert.False(result.Succeeded);
            }

            Assert.True(await userManager.IsLockedOutAsync(user));

            var correctLogin =
                await signInManager.CheckPasswordSignInAsync(
                    user,
                    correctPassword,
                    lockoutOnFailure: true);

            Assert.True(correctLogin.IsLockedOut);

            var lockoutEnd =
                await userManager.GetLockoutEndDateAsync(user);

            Assert.NotNull(lockoutEnd);

            var remaining =
                lockoutEnd.Value - DateTimeOffset.UtcNow;

            Assert.InRange(
                remaining.TotalMinutes,
                14,
                15.1);

            // Mo phong thoi gian khoa da ket thuc.
            var unlockResult = await userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddMinutes(-1));

            Assert.True(unlockResult.Succeeded);

            // Mat khau dung phai dang nhap duoc sau khi het khoa.
            var loginAfterUnlock =
                await signInManager.CheckPasswordSignInAsync(
                    user,
                    correctPassword,
                    lockoutOnFailure: true);

            Assert.True(loginAfterUnlock.Succeeded);

            // Sau khi dang nhap thanh cong, tai khoan khong con bi khoa.
            Assert.False(await userManager.IsLockedOutAsync(user));
        }
        finally
        {
            await userManager.DeleteAsync(user);
        }
    }
}
