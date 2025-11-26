namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Users.Application.Queries;
using CreateInvoiceSystem.Users.Application.RequestsResponses.GetUsers;
using MediatR;
using CreateInvoiceSystem.Abstractions.Mappers;

public class GetUserHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetUsersRequest, GetUsersResponse>
{
    public async Task<GetUsersResponse> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        GetUsersQuery query = new();

        List<User> Users = await queryExecutor.Execute(query);

        return new GetUsersResponse
        {
            Data = UserMappers.ToDtoList(Users)
        }; 
    }
}