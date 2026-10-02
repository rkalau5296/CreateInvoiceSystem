using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUser;

public class GetUserRequest(int id) : IRequest<GetUserResponse>
{
    public int Id { get; set; } = id;
}