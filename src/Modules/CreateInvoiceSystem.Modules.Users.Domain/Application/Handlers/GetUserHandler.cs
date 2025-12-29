using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
public class GetUserHandler(IQueryExecutor queryExecutor, IUserRepository _userRepository) : IRequestHandler<GetUserRequest, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        GetUserQuery query = new(request.Id);
        var user = await queryExecutor.Execute(query, _userRepository, cancellationToken);      

        return new GetUserResponse
        {
            Data = user.ToDto(),
        };
    }
}