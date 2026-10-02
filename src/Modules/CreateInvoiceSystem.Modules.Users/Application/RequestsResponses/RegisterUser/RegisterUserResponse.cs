using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;

public class RegisterUserResponse
{        
    public RegisterUserDto? Data { get; set; }            
}
