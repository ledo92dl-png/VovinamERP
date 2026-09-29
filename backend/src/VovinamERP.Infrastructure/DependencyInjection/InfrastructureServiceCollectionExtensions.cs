using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Finance.Common;
using VovinamERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using VovinamERP.Infrastructure.Identity;

namespace VovinamERP.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
    configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");
}

        services.AddDbContext<VovinamDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = false;

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
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<VovinamDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

        services.AddAuthentication();
        services.AddAuthorization();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VovinamDbContext>());
        services.AddScoped<
    ITuitionAdjustmentTransaction,
    TuitionAdjustmentTransaction>();
	services.AddScoped<
    IStudentCreditTransaction,
    StudentCreditLockTransaction>();
        services.AddScoped<
            VovinamERP.Application.Attendance.Common.IAttendanceRepository,
            VovinamERP.Infrastructure.Repositories.AttendanceRepository>();
        return services;
    }
}
