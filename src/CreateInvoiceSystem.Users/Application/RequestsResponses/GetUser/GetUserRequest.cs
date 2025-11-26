namespace CreateInvoiceSystem.Users.Application.RequestsResponses.GetUser;

using MediatR;

public class GetUserRequest(int id) : IRequest<GetUserResponse>
{
    public int Id { get; set; } = id;
}
