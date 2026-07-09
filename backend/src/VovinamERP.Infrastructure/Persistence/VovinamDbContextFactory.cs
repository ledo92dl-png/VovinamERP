using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class VovinamDbContextFactory : IDesignTimeDbContextFactory<VovinamDbContext>
{
    public VovinamDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VovinamDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("VOVINAMERP_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=vovinam_erp;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new VovinamDbContext(optionsBuilder.Options);
    }
}
