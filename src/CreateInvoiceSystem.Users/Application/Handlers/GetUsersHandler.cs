namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using MediatR;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Mappers;
using CreateInvoiceSystem.Modules.Users.Application.Queries;

public class GetUsersHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetUsersRequest, GetUsersResponse>
{
    public async Task<GetUsersResponse> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        GetUsersQuery query = new();

        List<User> users = await queryExecutor.Execute(query);

        var userList = UserMappers.ToDtoList(users);

        return new GetUsersResponse
        {
            Data = userList
        }; 
    }
}