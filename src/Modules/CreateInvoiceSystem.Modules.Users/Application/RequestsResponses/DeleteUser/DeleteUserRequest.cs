using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;

public class DeleteUserRequest(int id) : IRequest<DeleteUserResponse>, ITransactionalRequest
{
    public int Id { get; } = id;
}