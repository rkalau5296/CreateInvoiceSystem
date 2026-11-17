using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResetPassword
{
    public class ResetPasswordResponse : IApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}