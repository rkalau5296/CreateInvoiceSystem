using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CreateInvoiceSystem.Persistence;
using Testcontainers.MsSql;

namespace CreateInvoiceSystem.BuildTests.Transactions;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var databaseName =
            $"TransactionTests_{Guid.NewGuid():N}";

        var masterConnectionString =
            _container.GetConnectionString();

        await using (var connection =
            new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText =
                $"CREATE DATABASE [{databaseName}]";

            await command.ExecuteNonQueryAsync();
        }

        var builder =
            new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = databaseName
            };

        ConnectionString = builder.ConnectionString;

        var options = new DbContextOptionsBuilder<
            CreateInvoiceSystemDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context =
            new CreateInvoiceSystemDbContext(options);

        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}