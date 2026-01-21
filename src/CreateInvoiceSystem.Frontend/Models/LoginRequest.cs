using CreateInvoiceSystem.Frontend.Dto;

namespace CreateInvoiceSystem.Frontend.Models;

public class LoginRequest
{
    public LoginUserDto Dto { get; set; } = new LoginUserDto();
}
