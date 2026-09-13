using CreateInvoiceSystem.BuildTests.Transactions;

namespace CreateInvoiceSystem.BuildTests.Intergration
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        public SqlServerContainerFixture SqlServer { get; } = new();

        public TestWebApplicationFactory Factory { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await SqlServer.InitializeAsync();

            Factory = new TestWebApplicationFactory(
                SqlServer.ConnectionString);            
        }

        public async Task DisposeAsync()
        {
            Factory.Dispose();
            await SqlServer.DisposeAsync();
        }
    }
}
