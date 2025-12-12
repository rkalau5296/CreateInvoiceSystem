namespace CreateInvoiceSystem.Modules.Products.Services;

public interface IInvoicePositionReadService
{
    Task<bool> IsProductUsedAsync(int productId, CancellationToken cancellationToken = default);
}
