using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Users.Dto;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RegisterUser;

public class RegisterUserResponse
{        
    public RegisterUserDto? Data { get; set; }            
}
