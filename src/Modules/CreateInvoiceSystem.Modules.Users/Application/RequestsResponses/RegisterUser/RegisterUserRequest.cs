using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;


namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RegisterUser;

public class RegisterUserRequest : IRequest<RegisterUserResponse>, ITransactionalRequest
{
    public RegisterUserDto User { get; set; } = new();
}
