namespace CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateUserRequest(UserDto UserDto) : IRequest<CreateUserResponse>
{
    public UserDto User { get; } = UserDto;
}
