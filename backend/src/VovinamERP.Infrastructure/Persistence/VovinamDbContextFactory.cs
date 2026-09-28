using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class VovinamDbContextFactory : IDesignTimeDbContextFactory<VovinamDbContext>
{
    public VovinamDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VovinamDbContext>();

       var connectionString =
    Environment.GetEnvironmentVariable(
        "VOVINAMERP_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Environment variable 'VOVINAMERP_CONNECTION_STRING' is not configured.");
}

        optionsBuilder.UseNpgsql(connectionString);

        return new VovinamDbContext(optionsBuilder.Options);
    }
}
