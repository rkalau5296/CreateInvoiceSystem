using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
public class GetProductsHandler(IQueryExecutor _queryExecutor, IProductRepository _productRepository) : IRequestHandler<GetProductsRequest, GetProductsResponse>
{
    public async Task<GetProductsResponse> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        GetProductsQuery query = new(request.UserId, request.PageNumber, request.PageSize, request.SearchTerm);

       var products = await _queryExecutor.Execute(query, _productRepository, cancellationToken);

        return new GetProductsResponse
        {
            Data = ProductMappers.ToDtoList(products.Items),
            TotalCount = products.TotalCount
        };
    }
}