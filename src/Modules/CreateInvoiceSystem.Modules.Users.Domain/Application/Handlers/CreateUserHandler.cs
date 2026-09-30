using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class CreateUserHandler(IUserRepository _userRepository) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{   
    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = UserMappers.ToEntity(request.User);

        await _userRepository.AddAsync(user, cancellationToken);

        return new CreateUserResponse()
        {
            Data = UserMappers.ToCreateUserDto(user)
        };
    }
}