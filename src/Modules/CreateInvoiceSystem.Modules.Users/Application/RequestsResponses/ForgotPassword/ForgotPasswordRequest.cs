using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ForgotPassword;

public class ForgotPasswordRequest(ForgotPasswordDto dto) : IRequest<ForgotPasswordResponse>, ITransactionalRequest
{
    public ForgotPasswordDto Dto { get; } = dto;
}
