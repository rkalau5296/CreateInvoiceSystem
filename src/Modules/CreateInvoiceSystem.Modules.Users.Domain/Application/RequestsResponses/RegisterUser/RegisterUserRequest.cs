using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;


namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;

public class RegisterUserRequest : IRequest<RegisterUserResponse>
{
    public RegisterUserDto User { get; set; } = new();
}
