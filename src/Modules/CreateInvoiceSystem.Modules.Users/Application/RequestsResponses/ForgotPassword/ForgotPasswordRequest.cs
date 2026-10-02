using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;

public class ForgotPasswordRequest(ForgotPasswordDto dto) : IRequest<ForgotPasswordResponse>, ITransactionalRequest
{
    public ForgotPasswordDto Dto { get; } = dto;
}
