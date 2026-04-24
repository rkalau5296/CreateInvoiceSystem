using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class RegisterUserCommand : CommandBase<RegisterUserDto, RegisterUserDto, IUserRepository>
{
    private readonly IUserEmailSender _userEmailSender;
    private readonly IUserTokenService _userTokenService;
    private readonly IConfiguration _configuration;

    public RegisterUserCommand(
        IUserEmailSender userEmailSender,
        IUserTokenService userTokenService,
        IConfiguration configuration)
    {
        _userEmailSender = userEmailSender;
        _userTokenService = userTokenService;
        _configuration = configuration;
    }

    public override async Task<RegisterUserDto> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));

        var entity = UserMappers.ToEntity(this.Parametr);

        var result = await _userRepository.CreateWithPasswordAsync(entity, this.Parametr.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(TranslateIdentityError));
            throw new InvalidOperationException($"Rejestracja nieudana: {errors}");
        }

        var token = _userTokenService.GenerateActivationToken(entity.Email);
                
        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);
        if (!string.IsNullOrWhiteSpace(jti) && expiry.HasValue)
        {        
            var createdUser = await _userRepository.FindByEmailAsync(entity.Email);
            if (createdUser != null)
            {
                await _userRepository.SaveActivationTokenJtiAsync(createdUser.UserId, jti, expiry.Value, cancellationToken);
            }
        }

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var validatedUri))
        {
            throw new InvalidOperationException(
                $"BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny (Wartość: '{frontendUrl}'). " +
                "Sprawdź plik appsettings.json.");
        }

        var activationLink = $"{frontendUrl}/activate?token={Uri.EscapeDataString(token)}";

        await _userEmailSender.SendActivationEmailAsync(entity.Email, activationLink);

        return entity.ToRegisterUserDto();
    }

    private static string TranslateIdentityError(Microsoft.AspNetCore.Identity.IdentityError err)
    {
        var desc = err.Description ?? string.Empty;
        var code = err.Code ?? string.Empty;

        if (code.Equals("DuplicateEmail", StringComparison.OrdinalIgnoreCase)
            || desc.Contains("email", StringComparison.OrdinalIgnoreCase) && (desc.Contains("already", StringComparison.OrdinalIgnoreCase) || desc.Contains("in use", StringComparison.OrdinalIgnoreCase)))
        {
            return "Podany adres e‑mail jest już używany.";
        }

        if (code.Equals("DuplicateUserName", StringComparison.OrdinalIgnoreCase)
            || desc.Contains("user name", StringComparison.OrdinalIgnoreCase) && (desc.Contains("already", StringComparison.OrdinalIgnoreCase) || desc.Contains("in use", StringComparison.OrdinalIgnoreCase)))
        {
            return "Podana nazwa użytkownika jest już zajęta.";
        }

        if (code.Equals("InvalidEmail", StringComparison.OrdinalIgnoreCase))
            return "Niepoprawny format adresu e‑mail.";

        if (code.Equals("InvalidUserName", StringComparison.OrdinalIgnoreCase))
            return "Nieprawidłowa nazwa użytkownika.";

        if (code.Equals("PasswordTooShort", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(desc) ? "Hasło jest za krótkie." : desc;
        }

        if (code.Equals("PasswordRequiresNonAlphanumeric", StringComparison.OrdinalIgnoreCase))
            return "Hasło musi zawierać co najmniej jeden znak specjalny (np. !, @, #).";

        if (code.Equals("PasswordRequiresDigit", StringComparison.OrdinalIgnoreCase))
            return "Hasło musi zawierać co najmniej jedną cyfrę.";

        if (code.Equals("PasswordRequiresUpper", StringComparison.OrdinalIgnoreCase))
            return "Hasło musi zawierać co najmniej jedną wielką literę.";

        if (code.Equals("PasswordRequiresLower", StringComparison.OrdinalIgnoreCase))
            return "Hasło musi zawierać co najmniej jedną małą literę.";

        if (code.Equals("PasswordRequiresUniqueChars", StringComparison.OrdinalIgnoreCase))
            return "Hasło musi zawierać wymaganą liczbę różnych znaków.";

        if (!string.IsNullOrWhiteSpace(desc))
            return desc;

        return "Wystąpił błąd podczas rejestracji.";
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