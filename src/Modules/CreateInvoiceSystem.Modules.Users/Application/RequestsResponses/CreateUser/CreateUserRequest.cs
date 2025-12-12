namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.CreateUser;

using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;

public class CreateUserRequest(CreateUserDto UserDto) : IRequest<CreateUserResponse>
{
    public CreateUserDto User { get; } = UserDto;
}
