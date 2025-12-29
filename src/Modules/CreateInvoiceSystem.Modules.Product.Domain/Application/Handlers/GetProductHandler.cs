using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
public class GetProductHandler(IQueryExecutor queryExecutor, IProductRepository _productRepository) : IRequestHandler<GetProductRequest, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        GetProductQuery query = new(request.Id);
        var Product = await queryExecutor.Execute(query, _productRepository, cancellationToken);

        return new GetProductResponse
        {
            Data = ProductMappers.ToDto(Product),
        };
    }
}
