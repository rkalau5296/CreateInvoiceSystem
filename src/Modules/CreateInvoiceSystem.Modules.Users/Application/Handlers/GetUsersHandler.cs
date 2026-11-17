using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using CreateInvoiceSystem.Modules.Users.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;
public class GetUsersHandler(IUserRepository _userRepository) : IRequestHandler<GetUsersRequest, GetUsersResponse>
{
    public async Task<GetUsersResponse> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        List<User> users = await _userRepository.GetUsersAsync(cancellationToken)
            ?? throw new InvalidOperationException($"List of users is empty.");

        var userList = users.ToDtoList();

        return new GetUsersResponse
        {
            Data = userList
        }; 
    }
}