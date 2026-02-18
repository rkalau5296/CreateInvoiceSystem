using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class ActivateUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository, IUserTokenService _userTokenService)
    : IRequestHandler<ActivateUserRequest, ActivateUserResponse>
{
    public async Task<ActivateUserResponse> Handle(ActivateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(_userTokenService)
        {
            Parametr = request
        };

        return await commandExecutor.Execute(command, _userRepository, cancellationToken);       
    }
}
