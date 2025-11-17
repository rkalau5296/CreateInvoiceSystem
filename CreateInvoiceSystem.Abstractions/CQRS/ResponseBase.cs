namespace CreateInvoiceSystem.Abstractions.CQRS;

public abstract class ResponseBase<T>
{
    public T Data { get; set; } = default!;

}
