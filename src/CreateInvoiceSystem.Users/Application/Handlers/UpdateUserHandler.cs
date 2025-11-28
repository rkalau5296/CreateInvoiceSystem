namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Users.Application.Commands;
using CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;
using MediatR;


public class UpdateUserHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateUserRequest, UpdateUserResponse>
{    
    public async Task<UpdateUserResponse> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateUserCommand() { Parametr = request.User };
        
        var user = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateUserResponse()
        {
            Data = user
        };
    }
}
