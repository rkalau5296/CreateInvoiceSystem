using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class DeleteUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository) : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User { UserId = request.Id };

        var command = new DeleteUserCommand { Parametr = user };
        var deletedUser = await commandExecutor.Execute(command, _userRepository, cancellationToken);

        return new DeleteUserResponse()
        {
            Data = deletedUser
        };
    }
}

