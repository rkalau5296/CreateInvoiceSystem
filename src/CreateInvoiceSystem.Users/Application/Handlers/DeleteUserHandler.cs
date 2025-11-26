namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Users.Application.Commands;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Abstractions.Entities;
using MediatR;

public class DeleteUserHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var User = new User { UserId = request.Id };

        var command = new DeleteUserCommand { Parametr = User };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteUserResponse()
        {
            Data = UserMappers.ToDto(User)
        };
    }
}

