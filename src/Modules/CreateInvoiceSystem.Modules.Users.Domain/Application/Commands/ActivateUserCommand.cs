using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ActivateUserCommand : CommandBase<ActivateUserRequest, ActivateUserResponse, IUserRepository>
{
    private readonly IUserTokenService _userTokenService;

    public ActivateUserCommand(IUserTokenService userTokenService)
    {
        _userTokenService = userTokenService;
    }

    public override async Task<ActivateUserResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null || string.IsNullOrWhiteSpace(Parametr.Token))
            return new ActivateUserResponse { IsSuccess = false, Message = "Brak tokena aktywacyjnego." };

        var email = _userTokenService.GetEmailFromActivationToken(Parametr.Token);
        if (string.IsNullOrWhiteSpace(email))
            return new ActivateUserResponse { IsSuccess = false, Message = "Link wygasł lub jest nieprawidłowy." };

        var (jti, expiry) = ParseJtiAndExpiryFromJwt(Parametr.Token);
        if (string.IsNullOrWhiteSpace(jti))
            return new ActivateUserResponse { IsSuccess = false, Message = "Link wygasł lub jest nieprawidłowy." };

        var activated = await _userRepository.ValidateAndActivateUserByTokenAsync(email, jti, expiry, cancellationToken);
        if (!activated)
            return new ActivateUserResponse { IsSuccess = false, Message = "Link wygasł lub jest nieprawidłowy." };

        return new ActivateUserResponse { IsSuccess = true, Message = "Konto zostało aktywowane!" };
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