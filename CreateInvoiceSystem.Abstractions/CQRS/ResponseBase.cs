namespace CreateInvoiceSystem.Abstractions.CQRS;

using CreateInvoiceSystem.Abstractions.ErrorResponseBase;

public class ResponseBase<T> : ErrorResponseBase
{
    public T Data { get; set; } = default!;

}
