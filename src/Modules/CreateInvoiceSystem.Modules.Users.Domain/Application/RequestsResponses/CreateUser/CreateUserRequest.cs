using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;

public class CreateUserRequest(CreateUserDto UserDto) : IRequest<CreateUserResponse>
{
    public CreateUserDto User { get; } = UserDto;
}
