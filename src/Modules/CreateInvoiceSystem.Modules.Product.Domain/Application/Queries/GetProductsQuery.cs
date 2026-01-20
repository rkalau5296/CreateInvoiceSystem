using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
public class GetProductsQuery(int? userId, int pageNumber, int pageSize) : QueryBase<PagedResult<Product>, IProductRepository>
{
    public override async Task<PagedResult<Product>> Execute(IProductRepository _productRepository, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAllAsync(
            userId, pageNumber, pageSize, cancellationToken)
            ?? throw new InvalidOperationException($"List of products is empty.");
    }
}
