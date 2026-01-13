using CreateInvoiceSystem.Modules.Users.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;

public class RegisterUserResponse
{        
    public RegisterUserDto? Data { get; set; }        
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "User registered successfully.";
}
