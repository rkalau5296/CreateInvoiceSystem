using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.CreateUser;

public class CreateUserRequest(CreateUserDto UserDto) : IRequest<CreateUserResponse>, ITransactionalRequest
{
    public CreateUserDto User { get; } = UserDto;
}
