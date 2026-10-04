using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Entities;

namespace CreateInvoiceSystem.Modules.Users.Interfaces
{
    public interface IUserAuthService
    {
        AuthResponse GenerateAuthResponse(UserAuthModel user, Guid sessionId);
    }
}
