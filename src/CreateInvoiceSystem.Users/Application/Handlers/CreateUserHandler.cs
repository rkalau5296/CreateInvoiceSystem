namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.CreateUser;
using MediatR;

public class CreateUserHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{   
    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand() { Parametr = request.User };

        var createdUser = await commandExecutor.Execute(command, cancellationToken);

        return new CreateUserResponse()
        {
            Data = createdUser
        };
    }
}