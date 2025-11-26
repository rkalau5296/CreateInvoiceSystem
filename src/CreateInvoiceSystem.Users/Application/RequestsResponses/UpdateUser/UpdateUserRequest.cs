namespace CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateUserRequest(int id, UserDto UserDto) : IRequest<UpdateUserResponse>
{
    public UserDto User { get; } = UserDto with { UserId = id };
    public int Id { get; set; } = id;

}
