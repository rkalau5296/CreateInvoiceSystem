using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
public class DeleteUserRequest(int id) : IRequest<DeleteUserResponse>
{    
    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}