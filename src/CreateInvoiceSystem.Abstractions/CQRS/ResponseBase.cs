namespace CreateInvoiceSystem.Abstractions.CQRS;

public class ResponseBase<T> 
{
    public T Data { get; set; } = default!;
}
