using CreateInvoiceSystem.Identity.Models;

namespace CreateInvoiceSystem.Identity.Interfaces;

public interface IJwtProvider
{
    TokenResponse Generate(IdentityUserModel userModel);
}
