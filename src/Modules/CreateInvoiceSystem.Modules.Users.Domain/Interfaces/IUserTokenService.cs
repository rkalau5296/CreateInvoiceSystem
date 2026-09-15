namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserTokenService
{
    (string AccessToken, Guid RefreshToken) CreateToken(int userId, string email, string company, string nip, List<string> roles, Guid sessionId);
    string GenerateActivationToken(string email);
    string GenerateActivationToken(string email, int expiresHours);
    string? GetEmailFromActivationToken(string token);
}
