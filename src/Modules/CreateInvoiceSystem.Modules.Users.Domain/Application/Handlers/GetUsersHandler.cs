using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
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