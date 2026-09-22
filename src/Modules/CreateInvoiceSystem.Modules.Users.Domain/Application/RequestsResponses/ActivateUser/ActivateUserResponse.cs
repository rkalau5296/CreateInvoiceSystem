using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;

public record ActivateUserResponse : IApiResponse
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
}
