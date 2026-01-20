using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces
{
    public interface IUserAuthService
    {
        AuthResponse GenerateAuthResponse(UserAuthModel user);
    }
}
