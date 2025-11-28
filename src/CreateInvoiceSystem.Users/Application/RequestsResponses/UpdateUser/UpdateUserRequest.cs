namespace CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateUserRequest(int id, UpdateUserDto userDto) : IRequest<UpdateUserResponse>
{
    public UpdateUserDto User { get; } = userDto with { UserId = id };
    public int Id { get; set; } = id;

}
