namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;

using MediatR;

public class DeleteUserRequest(int id) : IRequest<DeleteUserResponse>
{    
    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}