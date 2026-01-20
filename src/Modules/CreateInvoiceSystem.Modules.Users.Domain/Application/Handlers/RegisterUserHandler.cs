using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class RegisterUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository)
    : IRequestHandler<RegisterUserRequest, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {        
        var command = new RegisterUserCommand { Parametr = request.User };        
        var createdUser = await commandExecutor.Execute(command, _userRepository, cancellationToken);
        
        return new RegisterUserResponse
        {
            Data = createdUser
        };
    }
}
