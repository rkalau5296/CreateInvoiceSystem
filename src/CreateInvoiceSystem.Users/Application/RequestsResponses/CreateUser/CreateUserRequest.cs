namespace CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateUserRequest(CreateUserDto UserDto) : IRequest<CreateUserResponse>
{
    public CreateUserDto User { get; } = UserDto;
}
