using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class UserCleanupServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserEmailSender> _emailSenderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<ILogger<Modules.Users.Domain.Application.Services.UserCleanupService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IUserTokenService> _userTokenServiceMock;

    public UserCleanupServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailSenderMock = new Mock<IUserEmailSender>();
        _emailSenderMock = new Mock<IUserEmailSender>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _loggerMock = new Mock<ILogger<Modules.Users.Domain.Application.Services.UserCleanupService>>();
        _configurationMock = new Mock<IConfiguration>();
        _userTokenServiceMock = new Mock<IUserTokenService>();
        _userTokenServiceMock = new Mock<IUserTokenService>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository))).Returns(_userRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserEmailSender))).Returns(_emailSenderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IConfiguration))).Returns(_configurationMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserTokenService))).Returns(_userTokenServiceMock.Object);

        _configurationMock.Setup(c => c["FrontendUrl"]).Returns("https://localhost:7022");
        _userTokenServiceMock.Setup(t => t.GenerateActivationToken(It.IsAny<string>())).Returns("raw-token");
    }

    [Fact]
    public async Task ExecuteAsync_Should_PerformCleanupAndSendWarnings()
    {
        var testUsers = new List<User> { new User { Email = "test@example.com", Name = "Test" } };

        _userRepositoryMock
            .Setup(x => x.GetUsersForCleanupWarningAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUsers);

        _userRepositoryMock
            .Setup(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new Modules.Users.Domain.Application.Services.UserCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        using var cts = new CancellationTokenSource();

        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(300);
        cts.Cancel();
        await executeTask;

        _userRepositoryMock.Verify(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _emailSenderMock.Verify(x => x.SendCleanupWarningEmailAsync(
            "test@example.com",
            "Test",
            5,
            It.Is<string>(link => !string.IsNullOrWhiteSpace(link))
        ), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ContinueRunning_When_ExceptionOccurs()
    {
        _userRepositoryMock
            .Setup(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _userRepositoryMock
            .Setup(x => x.GetUsersForCleanupWarningAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var service = new Modules.Users.Domain.Application.Services.UserCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        using var cts = new CancellationTokenSource();

        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(300);
        cts.Cancel();
        await executeTask;

        _userRepositoryMock.Verify(x => x.RemoveInactiveUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}