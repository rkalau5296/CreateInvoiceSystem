using CreateInvoiceSystem.Modules.Users.Application.Services;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
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
    private readonly Mock<ILogger<UserCleanupService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IUserTokenService> _userTokenServiceMock;

    public UserCleanupServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailSenderMock = new Mock<IUserEmailSender>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _loggerMock = new Mock<ILogger<UserCleanupService>>();
        _configurationMock = new Mock<IConfiguration>();
        _userTokenServiceMock = new Mock<IUserTokenService>();

        _scopeFactoryMock
            .Setup(factory => factory.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock
            .Setup(scope => scope.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _serviceProviderMock
            .Setup(provider => provider.GetService(
                typeof(IUserRepository)))
            .Returns(_userRepositoryMock.Object);

        _serviceProviderMock
            .Setup(provider => provider.GetService(
                typeof(IUserEmailSender)))
            .Returns(_emailSenderMock.Object);

        _serviceProviderMock
            .Setup(provider => provider.GetService(
                typeof(IConfiguration)))
            .Returns(_configurationMock.Object);

        _serviceProviderMock
            .Setup(provider => provider.GetService(
                typeof(IUserTokenService)))
            .Returns(_userTokenServiceMock.Object);

        _configurationMock
            .Setup(configuration => configuration["FrontendUrl"])
            .Returns("https://localhost:7022");

        _userTokenServiceMock
            .Setup(tokenService =>
                tokenService.GenerateActivationToken(
                    It.IsAny<string>()))
            .Returns("raw-token");
    }

    [Fact]
    public async Task ExecuteAsync_Should_PerformCleanupAndSendWarnings()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var testUsers = new List<User>
        {
            new User
            {
                Email = "test@example.com",
                Name = "Test"
            }
        };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetUsersForCleanupWarningAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUsers);

        _userRepositoryMock
            .Setup(repository =>
                repository.RemoveInactiveUsersAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new UserCleanupService(
            _scopeFactoryMock.Object,
            _loggerMock.Object);

        var executeTask = service.StartAsync(cancellationToken);

        await Task.Delay(
            TimeSpan.FromMilliseconds(300),
            cancellationToken);

        await service.StopAsync(cancellationToken);
        await executeTask;

        _userRepositoryMock.Verify(
            repository =>
                repository.RemoveInactiveUsersAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        _emailSenderMock.Verify(
            emailSender =>
                emailSender.SendCleanupWarningEmailAsync(
                    "test@example.com",
                    "Test",
                    5,
                    It.Is<string>(
                        link => !string.IsNullOrWhiteSpace(link))),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ContinueRunning_When_ExceptionOccurs()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        _userRepositoryMock
            .Setup(repository =>
                repository.RemoveInactiveUsersAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _userRepositoryMock
            .Setup(repository =>
                repository.GetUsersForCleanupWarningAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var service = new UserCleanupService(
            _scopeFactoryMock.Object,
            _loggerMock.Object);

        var executeTask = service.StartAsync(cancellationToken);

        await Task.Delay(
            TimeSpan.FromMilliseconds(300),
            cancellationToken);

        await service.StopAsync(cancellationToken);
        await executeTask;

        _userRepositoryMock.Verify(
            repository =>
                repository.RemoveInactiveUsersAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}