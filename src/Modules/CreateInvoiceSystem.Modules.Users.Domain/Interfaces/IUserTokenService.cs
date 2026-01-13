namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserTokenService
{
    string CreateToken(int userId, string email, string company, string nip, List<string> roles);
}
