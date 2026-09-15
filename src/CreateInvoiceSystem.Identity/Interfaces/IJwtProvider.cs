using CreateInvoiceSystem.Identity.Models;

namespace CreateInvoiceSystem.Identity.Interfaces;

public interface IJwtProvider
{
    TokenResponse Generate(IdentityUserModel userModel, Guid refreshToken, Guid sessionId);
    string GenerateActivationToken(string email, int expiresHours);
    string? GetEmailFromActivationToken(string token);
}