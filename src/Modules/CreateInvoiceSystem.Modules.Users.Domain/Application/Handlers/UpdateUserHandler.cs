using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class UpdateUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository) : IRequestHandler<UpdateUserRequest, UpdateUserResponse>
{    
    public async Task<UpdateUserResponse> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateUserCommand() { Parametr = request.User };
        
        var user = await commandExecutor.Execute(command, _userRepository, cancellationToken);        

        return new UpdateUserResponse()
        {
            Data = user
        };
    }
}
