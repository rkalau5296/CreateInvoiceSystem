using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUser;

public class GetUserRequest(int id) : IRequest<GetUserResponse>
{
    public int Id { get; set; } = id;
}