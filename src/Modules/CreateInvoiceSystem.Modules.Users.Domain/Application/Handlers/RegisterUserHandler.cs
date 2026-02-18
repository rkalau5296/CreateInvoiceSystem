using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class RegisterUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository, IUserEmailSender _emailSender, IUserTokenService _userTokenService, IConfiguration _configuration)
    : IRequestHandler<RegisterUserRequest, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {        
        var command = new RegisterUserCommand(_emailSender, _userTokenService, _configuration) { Parametr = request.User };        
        var createdUser = await commandExecutor.Execute(command, _userRepository, cancellationToken);
        
        return new RegisterUserResponse
        {
            Data = createdUser
        };
    }
}
