using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ChangePassword;

public record ChangePasswordRequest(ChangePasswordDto Dto) : IRequest<ChangePasswordResponse>, ITransactionalRequest
{
}
