namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Users.Application.Commands;
using CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;
using MediatR;

public class CreateUserHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{   
    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand() { Parametr = request.User };

        var UserFromDb = await commandExecutor.Execute(command, cancellationToken);

        return new CreateUserResponse()
        {
            Data = UserFromDb
        };
    }
}