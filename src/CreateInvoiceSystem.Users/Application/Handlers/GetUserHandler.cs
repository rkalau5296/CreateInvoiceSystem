namespace CreateInvoiceSystem.Users.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Users.Application.Queries;
using CreateInvoiceSystem.Users.Application.RequestsResponses.GetUser;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetUserHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetUserRequest, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        GetUserQuery query = new(request.Id);
        var user = await queryExecutor.Execute(query);      

        return new GetUserResponse
        {
            Data = UserMappers.ToDto(user),
        };
    }
}