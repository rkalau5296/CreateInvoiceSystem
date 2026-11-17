using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ActivateUser;

public record ActivateUserResponse : IApiResponse
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
}
