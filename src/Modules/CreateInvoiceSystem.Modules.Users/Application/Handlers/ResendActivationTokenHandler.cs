using System.Text;
using System.Text.Json;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResendToken;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

public class ResendActivationTokenHandler(
    IUserRepository userRepository,
    IUserTokenService userTokenService,
    IUserEmailSender userEmailSender,
    IConfiguration configuration)
    : IRequestHandler<ResendActivationTokenRequest, ResendActivationTokenResponse>
{
    public async Task<ResendActivationTokenResponse> Handle(
        ResendActivationTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request?.Email))
        {
            return new ResendActivationTokenResponse
            {
                IsSuccess = false,
                Message = "Email jest wymagany."
            };
        }

        var user = await userRepository.FindByEmailAsync(request.Email);
                
        if (user == null)
        {
            return new ResendActivationTokenResponse
            {
                IsSuccess = true,
                Message = "Jeśli konto istnieje i nie jest aktywne, nowy link został wysłany."
            };
        }

        if (user.IsActive)
        {
            return new ResendActivationTokenResponse
            {
                IsSuccess = false,
                Message = "To konto jest już aktywne."
            };
        }

        var token = userTokenService.GenerateActivationToken(user.Email);

        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);
        if (!string.IsNullOrWhiteSpace(jti) && expiry.HasValue)
        {
            await userRepository.SaveActivationTokenJtiAsync(user.UserId, jti, expiry.Value, cancellationToken);
        }

        var frontendUrl = configuration["FrontendUrl"]?.TrimEnd('/');

        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                $"BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny (Wartość: '{frontendUrl}'). " +
                "Sprawdź plik appsettings.json.");
        }

        var activationLink = $"{frontendUrl}/activate?token={Uri.EscapeDataString(token)}";

        await userEmailSender.SendActivationEmailAsync(user.Email, activationLink);

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