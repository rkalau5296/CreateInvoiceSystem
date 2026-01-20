using CreateInvoiceSystem.Abstractions.Executors;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Base;

public abstract class BaseTest<TRepository> where TRepository : class
{
    protected readonly Mock<ICommandExecutor> ExecutorMock;
    protected readonly Mock<TRepository> RepositoryMock;
    protected readonly CancellationToken CancellationToken;

    protected BaseTest()
    {
        ExecutorMock = new Mock<ICommandExecutor>();
        RepositoryMock = new Mock<TRepository>(); 
        CancellationToken = CancellationToken.None;
    }
}