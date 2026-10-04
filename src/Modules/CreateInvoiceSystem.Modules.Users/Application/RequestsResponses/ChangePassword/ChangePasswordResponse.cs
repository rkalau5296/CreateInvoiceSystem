using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ChangePassword
{
    public record ChangePasswordResponse(bool IsSuccess, string Message): IApiResponse;    
}