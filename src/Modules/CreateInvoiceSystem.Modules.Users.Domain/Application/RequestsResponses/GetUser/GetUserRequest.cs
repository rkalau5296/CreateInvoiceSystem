using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUser;
public class GetUserRequest(int id) : IRequest<GetUserResponse>
{
    public int Id { get; set; } = id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
