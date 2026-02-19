using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreateInvoiceSystem.BuildTests.UserCleanupService;

public class UserCleanupServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserEmailSender> _emailSenderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<ILogger<Modules.Users.Domain.Application.Services.UserCleanupService>> _loggerMock;

    public UserCleanupServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailSenderMock = new Mock<IUserEmailSender>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _loggerMock = new Mock<ILogger<Modules.Users.Domain.Application.Services.UserCleanupService>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository))).Returns(_userRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserEmailSender))).Returns(_emailSenderMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PerformCleanupAndSendWarnings()
    {
        // Arrange
        var service = new Modules.Users.Domain.Application.Services.UserCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        using var cts = new CancellationTokenSource();
        var testUsers = new List<User> { new User { Email = "test@example.com", Name = "Test" } };

        _userRepositoryMock
            .Setup(x => x.GetUsersForCleanupWarningAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUsers);

        // Act
        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();
        await executeTask;

        // Assert
        _userRepositoryMock.Verify(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _emailSenderMock.Verify(x => x.SendCleanupWarningEmailAsync("test@example.com", "Test", 5), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ContinueRunning_When_ExceptionOccurs()
    {
        // Arrange
        var service = new Modules.Users.Domain.Application.Services.UserCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        using var cts = new CancellationTokenSource();

        _userRepositoryMock
            .Setup(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();
        await executeTask;

        // Assert
        _userRepositoryMock.Verify(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
