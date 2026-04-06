namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public class ActivateUserCommand : CommandBase<ActivateUserRequest, ActivateUserResponse, IUserRepository>
{
    private readonly IUserTokenService _userTokenService;

    public ActivateUserCommand(IUserTokenService userTokenService)
    {
        _userTokenService = userTokenService;
    }

    public override async Task<ActivateUserResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        var token = this.Parametr?.Token;
                
        if (string.IsNullOrWhiteSpace(token))
            return new ActivateUserResponse { IsSuccess = false, Message = "Błąd: Brak tokena aktywacyjnego." };
                
        var email = _userTokenService.GetEmailFromActivationToken(token);
        if (string.IsNullOrWhiteSpace(email))
            return new ActivateUserResponse { IsSuccess = false, Message = "Błąd: Link wygasł lub jest nieprawidłowy." };
                
        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);
        if (string.IsNullOrWhiteSpace(jti) || expiry == null || expiry.Value <= DateTimeOffset.UtcNow)
            return new ActivateUserResponse { IsSuccess = false, Message = "Błąd: Link wygasł lub jest nieprawidłowy." };
                
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
            return new ActivateUserResponse { IsSuccess = false, Message = "Błąd: Użytkownik nie istnieje." };
                
        if (user.IsActive)
            return new ActivateUserResponse { IsSuccess = true, Message = "Konto jest już aktywne." };
                
        var activated = await _userRepository.ValidateAndActivateUserByTokenAsync(email, jti, expiry, cancellationToken);
        if (!activated)
            return new ActivateUserResponse { IsSuccess = false, Message = "Błąd: Link wygasł lub jest nieprawidłowy." };

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