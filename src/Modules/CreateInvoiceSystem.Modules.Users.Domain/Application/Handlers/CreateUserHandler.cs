using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class CreateUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{   
    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand() { Parametr = request.User };

        var createdUser = await commandExecutor.Execute(command, _userRepository, cancellationToken);

        return new CreateUserResponse()
        {
            Data = createdUser
        };
    }
}