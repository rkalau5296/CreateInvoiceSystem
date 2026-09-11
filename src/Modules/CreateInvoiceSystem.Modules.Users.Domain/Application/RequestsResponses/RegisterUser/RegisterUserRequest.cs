using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;


namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;

public class RegisterUserRequest : IRequest<RegisterUserResponse>, ITransactionalRequest
{
    public RegisterUserDto User { get; set; } = new();
}
