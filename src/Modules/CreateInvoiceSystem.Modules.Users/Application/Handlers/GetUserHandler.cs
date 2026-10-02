using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class GetUserHandler(IUserRepository _userRepository) : IRequestHandler<GetUserRequest, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {request.Id} not found.");

        return new GetUserResponse
        {
            Data = user.ToDto(),
        };
    }
}