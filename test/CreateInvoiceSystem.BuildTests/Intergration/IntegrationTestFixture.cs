using CreateInvoiceSystem.BuildTests.Transactions;
using Microsoft.Data.SqlClient;
using Respawn;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public SqlServerContainerFixture SqlServer { get; } = new();

    public TestWebApplicationFactory Factory { get; private set; } = null!;

    private SqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        await SqlServer.InitializeAsync();

        Factory = new TestWebApplicationFactory(
            SqlServer.ConnectionString);

        _connection = new SqlConnection(SqlServer.ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                WithReseed = true
            });
    }

    public Task ResetDatabaseAsync()
    {
        return _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();

        await _connection.DisposeAsync();
        await SqlServer.DisposeAsync();
    }
}