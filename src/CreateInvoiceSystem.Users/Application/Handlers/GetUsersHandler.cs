namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Users.Application.Queries;
using CreateInvoiceSystem.Users.Application.RequestsResponses.GetUsers;
using MediatR;
using CreateInvoiceSystem.Abstractions.Mappers;

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