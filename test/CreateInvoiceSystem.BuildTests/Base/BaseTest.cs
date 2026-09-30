using Moq;

namespace CreateInvoiceSystem.BuildTests.Base;

public abstract class BaseTest<TRepository> where TRepository : class
{    
    protected readonly Mock<TRepository> RepositoryMock;
    protected readonly CancellationToken CancellationToken;

    protected BaseTest()
    {        
        RepositoryMock = new Mock<TRepository>(); 
        CancellationToken = CancellationToken.None;
    }
}