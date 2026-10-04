using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ActivateUser;

public class ActivateUserRequest : IRequest<ActivateUserResponse>, ITransactionalRequest
{
    public string Token { get; set; } = string.Empty;
}
