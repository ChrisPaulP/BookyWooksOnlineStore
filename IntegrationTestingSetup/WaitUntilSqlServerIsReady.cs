using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;

namespace IntegrationTestingSetup;

public class WaitUntilSqlServerIsReady : IWaitUntil
{
    public async Task<bool> UntilAsync(IContainer container, CancellationToken ct = default)
    {
        var host = container.Hostname;
        var port = container.GetMappedPublicPort(1433);
        var connectionString =
            $"Server={host},{port};User Id=sa;Password=Your_password123;TrustServerCertificate=True;Connection Timeout=3";

        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            return true;
        }
        catch
        {
            return false; // Retry until ready
        }
    }

    public Task<bool> UntilAsync(IContainer container)
    {
        throw new NotImplementedException();
    }
}