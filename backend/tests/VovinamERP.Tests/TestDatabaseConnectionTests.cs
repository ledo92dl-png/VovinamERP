using Microsoft.EntityFrameworkCore;

namespace VovinamERP.Tests;

public class TestDatabaseConnectionTests
{
    [Fact]
    public async Task ShouldConnectToTestDatabase()
    {
        await using var db = TestDatabase.CreateContext();

        var connection = db.Database.GetDbConnection();

        Assert.Equal(
            "vovinam_erp_test",
            connection.Database);

        await connection.OpenAsync();

        Assert.Equal(
            System.Data.ConnectionState.Open,
            connection.State);
    }
}