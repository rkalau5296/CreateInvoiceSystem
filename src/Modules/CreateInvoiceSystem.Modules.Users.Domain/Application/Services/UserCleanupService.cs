using System.Text;
using System.Text.Json;
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
                    try
                    {
                        var daysLeft = 5;

                        var token = _userTokenService.GenerateActivationToken(user.Email);
                        if (string.IsNullOrWhiteSpace(token))
                            continue;

                        var (jti, expiryUtc) = ParseJtiAndExpiryFromJwt(token);

                        if (!string.IsNullOrWhiteSpace(jti) && expiryUtc != null)
                        {
                            await _userRepository.SaveActivationTokenJtiAsync(user.UserId, jti, expiryUtc.Value, cancellationToken);
                        }

                        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

                        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var validatedUri))
                        {
                            throw new InvalidOperationException(
                                $"BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny (Wartość: '{frontendUrl}'). " +
                                "Sprawdź plik appsettings.json.");
                        }

                        var activationLink = $"{frontendUrl}/activate?token={Uri.EscapeDataString(token)}";

                        await _emailService.SendCleanupWarningEmailAsync(user.Email, user.Name, daysLeft, activationLink);
                    }
                    catch (Exception exUser)
                    {
                        _logger.LogError(exUser, "UserCleanupService: failed to generate/send activation link for user {Email}.", user.Email);
                    }
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

    private static (string? jti, DateTimeOffset? expiryUtc) ParseJtiAndExpiryFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return (null, null);

            string payload = parts[1];            
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
                case 0: break;
                default: break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? jti = null;
            DateTimeOffset? expiry = null;

            if (root.TryGetProperty("jti", out var jtiProp) && jtiProp.ValueKind == JsonValueKind.String)
            {
                jti = jtiProp.GetString();
            }

            if (root.TryGetProperty("exp", out var expProp) && expProp.ValueKind == JsonValueKind.Number)
            {
                if (expProp.TryGetInt64(out var expSeconds))
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                    expiry = new DateTimeOffset(dt);
                }
            }

            return (jti, expiry);
        }
        catch
        {
            return (null, null);
        }
    }
}