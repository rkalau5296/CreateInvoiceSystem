namespace CreateInvoiceSystem.BuildTests.Integration
{
    [CollectionDefinition("Integration tests", DisableParallelization = true)]
    public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
