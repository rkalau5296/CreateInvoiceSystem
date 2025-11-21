namespace CreateInvoiceSystem.Abstractions.ErrorResponseBase;

public class ErrorModel(string error)
{
    public string Error { get; set; } = error;
}