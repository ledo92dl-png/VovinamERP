
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Tests;

public static class TestDatabase
{
    public static VovinamDbContext CreateContext()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("VOVINAM_TEST_DB");

        var password =
            Environment.GetEnvironmentVariable("VOVINAM_TEST_DB_PASSWORD");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "Test database configuration is missing.");
        }

        var builder =
            new NpgsqlConnectionStringBuilder(connectionString)
            {
                Password = password
            };

        if (!string.Equals(
                builder.Database,
                "vovinam_erp_test",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Tests must use vovinam_erp_test.");
        }

        var options =
            new DbContextOptionsBuilder<VovinamDbContext>()
                .UseNpgsql(builder.ConnectionString)
                .Options;

        return new VovinamDbContext(options);
    }
}