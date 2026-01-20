using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
public class GetProductQuery(int id, int? userId) : QueryBase<Product, IProductRepository>
{
    public override async Task<Product> Execute(IProductRepository productRepository, CancellationToken cancellationToken = default)
    {
        return await productRepository.GetByIdAsync(id, userId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {id} not found.");
    }
}