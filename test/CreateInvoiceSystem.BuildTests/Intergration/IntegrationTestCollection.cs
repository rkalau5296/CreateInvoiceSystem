namespace CreateInvoiceSystem.BuildTests.Intergration
{
    [CollectionDefinition("Integration tests", DisableParallelization = true)]
    public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
