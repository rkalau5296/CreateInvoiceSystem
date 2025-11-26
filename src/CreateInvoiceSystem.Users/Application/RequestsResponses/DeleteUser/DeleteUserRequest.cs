namespace CreateInvoiceSystem.Users.Application.RequestsResponses.DeleteUser;

using MediatR;

public class DeleteUserRequest(int id) : IRequest<DeleteUserResponse>
{
    public int Id { get; } = id;
}
