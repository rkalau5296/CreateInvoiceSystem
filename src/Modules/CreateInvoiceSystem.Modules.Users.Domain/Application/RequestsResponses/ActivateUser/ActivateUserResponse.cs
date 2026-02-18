using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;

public class ActivateUserResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
