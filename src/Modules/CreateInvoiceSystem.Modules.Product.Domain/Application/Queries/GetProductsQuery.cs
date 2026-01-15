using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
public class GetProductsQuery(int? userId) : QueryBase<List<Product>, IProductRepository>
{
    public override async Task<List<Product>> Execute(IProductRepository productRepository, CancellationToken cancellationToken = default)
    {
        return await productRepository.GetAllAsync(
            userId, cancellationToken)
            ?? throw new InvalidOperationException($"List of products is empty.");
    }
}
