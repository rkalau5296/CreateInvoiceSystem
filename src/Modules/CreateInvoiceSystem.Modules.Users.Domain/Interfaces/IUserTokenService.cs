namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserTokenService
{
    (string AccessToken, Guid RefreshToken) CreateToken(int userId, string email, string company, string nip, List<string> roles);
    string GenerateActivationToken(string email);
    string? GetEmailFromActivationToken(string token);
}
