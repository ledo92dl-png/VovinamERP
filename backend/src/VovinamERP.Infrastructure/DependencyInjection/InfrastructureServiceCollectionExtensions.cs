using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=vovinam_erp;Username=postgres;Password=postgres";

        services.AddDbContext<VovinamDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VovinamDbContext>());
        services.AddScoped<
            VovinamERP.Application.Attendance.Common.IAttendanceRepository,
            VovinamERP.Infrastructure.Repositories.AttendanceRepository>();
        return services;
    }
}
