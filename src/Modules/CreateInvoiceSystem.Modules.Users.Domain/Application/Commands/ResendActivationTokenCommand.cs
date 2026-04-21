using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ResendActivationTokenCommand : CommandBase<ResendActivationTokenRequest, ResendActivationTokenResponse, IUserRepository>
{
    private readonly IUserTokenService _userTokenService;
    private readonly IUserEmailSender _userEmailSender;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public ResendActivationTokenCommand(
        IUserTokenService userTokenService,
        IUserEmailSender userEmailSender,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _userTokenService = userTokenService;
        _userEmailSender = userEmailSender;
        _configuration = configuration;
    }

    public override async Task<ResendActivationTokenResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(this.Parametr?.Email))
            return new ResendActivationTokenResponse { IsSuccess = false, Message = "Email jest wymagany." };

        var user = await _userRepository.FindByEmailAsync(this.Parametr.Email);

        if (user == null)
        {
            return new ResendActivationTokenResponse { IsSuccess = true, Message = "Jeśli konto istnieje i nie jest aktywne, nowy link został wysłany." };
        }

        if (user.IsActive)
        {
            return new ResendActivationTokenResponse { IsSuccess = false, Message = "To konto jest już aktywne." };
        }

        var token = _userTokenService.GenerateActivationToken(user.Email);
                
        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);
        if (!string.IsNullOrWhiteSpace(jti) && expiry.HasValue)
        {
            await _userRepository.SaveActivationTokenJtiAsync(user.UserId, jti, expiry.Value, cancellationToken);
        }

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var validatedUri))
        {
            throw new InvalidOperationException(
                $"BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny (Wartość: '{frontendUrl}'). " +
                "Sprawdź plik appsettings.json.");
        }

        var activationLink = $"{frontendUrl}/activate?token={Uri.EscapeDataString(token)}";

        await _userEmailSender.SendActivationEmailAsync(user.Email, activationLink);

        return new ResendActivationTokenResponse
        {
            IsSuccess = true,
            Message = "Nowy link aktywacyjny został wysłany na Twój e-mail."
        };
    }

    private static (string? jti, DateTimeOffset? expiryUtc) ParseJtiAndExpiryFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return (null, null);

            var payload = parts[1];
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
                jti = jtiProp.GetString();

            if (root.TryGetProperty("exp", out var expProp) && expProp.ValueKind == JsonValueKind.Number
                && expProp.TryGetInt64(out var expSeconds))
                expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds);

            return (jti, expiry);
        }
        catch
        {
            return (null, null);
        }
    }
}