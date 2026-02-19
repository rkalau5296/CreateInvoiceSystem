using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
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

                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                await _userRepository.RemoveInactiveUsersAsync(cutoffDate, cancellationToken);

                var warningDate = DateTime.UtcNow.AddDays(-25);
                var usersToWarn = await _userRepository.GetUsersForCleanupWarningAsync(warningDate, cancellationToken);

                foreach (var user in usersToWarn)
                {
                    await _emailService.SendCleanupWarningEmailAsync(user.Email, user.Name, 5);
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
