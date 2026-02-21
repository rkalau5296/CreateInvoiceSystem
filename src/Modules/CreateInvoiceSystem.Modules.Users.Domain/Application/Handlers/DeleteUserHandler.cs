using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class DeleteUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository, IMediator _mediator) : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Publish(new UserDeletedNotification(request.Id), cancellationToken);

        var user = new User { UserId = request.Id };

        var command = new DeleteUserCommand { Parametr = user };
        var deletedUser = await commandExecutor.Execute(command, _userRepository, cancellationToken);

        return new DeleteUserResponse()
        {
            Data = deletedUser
        };
    }
}

