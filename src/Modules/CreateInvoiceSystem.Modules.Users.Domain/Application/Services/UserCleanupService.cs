using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Services;

public class UserCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserCleanupService> _logger;

    public UserCleanupService(IServiceScopeFactory scopeFactory, ILogger<UserCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var _emailService = scope.ServiceProvider.GetRequiredService<IUserEmailSender>();
                var _userTokenService = scope.ServiceProvider.GetRequiredService<IUserTokenService>();
                var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var removedCount = await _userRepository.RemoveInactiveUsersAsync(cutoffDate, cancellationToken);

                _logger.LogInformation("UserCleanupService: removed {RemovedCount} inactive users older than {CutoffDate}.", removedCount, cutoffDate);

                var warningDate = DateTime.UtcNow.AddDays(-25);
                var usersToWarn = await _userRepository.GetUsersForCleanupWarningAsync(warningDate, cancellationToken);

                foreach (var user in usersToWarn)
                {
                    var token = _userTokenService.GenerateActivationToken(user.Email);
                    var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "https://localhost:7022";
                    var activationLink = $"{frontendUrl}/activate?token={System.Uri.EscapeDataString(token)}";                    
                    await _emailService.SendCleanupWarningEmailAsync(user.Email, user.Name, 5, activationLink);
                }

                if (usersToWarn.Any())
                {
                    _logger.LogInformation("Wysłano {Count} przypomnień o usunięciu konta.", usersToWarn.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd w UserCleanupService.");
            }

            await Task.Delay(TimeSpan.FromHours(24), cancellationToken);
        }
    }
}
