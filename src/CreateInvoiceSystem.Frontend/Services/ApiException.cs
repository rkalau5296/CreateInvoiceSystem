namespace CreateInvoiceSystem.Frontend.Services;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Response { get; }

    public ApiException(string message, int statusCode = 500, string? response = null) : base(message)
    {
        StatusCode = statusCode;
        Response = response;
    }
}
