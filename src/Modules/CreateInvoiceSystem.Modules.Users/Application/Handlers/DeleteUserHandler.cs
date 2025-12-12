namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using MediatR;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Application.Commands;

public class DeleteUserHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User { UserId = request.Id };

        var command = new DeleteUserCommand { Parametr = user };
        var deletedUser = await commandExecutor.Execute(command, cancellationToken);

        return new DeleteUserResponse()
        {
            Data = deletedUser
        };
    }
}

