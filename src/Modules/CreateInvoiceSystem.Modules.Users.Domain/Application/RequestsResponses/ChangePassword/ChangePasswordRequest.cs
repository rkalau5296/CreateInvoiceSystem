using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ChangePassword;

public record ChangePasswordRequest(ChangePasswordDto Dto) : IRequest<ChangePasswordResponse>
{
}
