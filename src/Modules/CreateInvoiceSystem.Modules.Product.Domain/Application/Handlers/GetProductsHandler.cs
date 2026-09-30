using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;


namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
public class GetProductsHandler(IProductRepository _productRepository) : IRequestHandler<GetProductsRequest, GetProductsResponse>
{
    public async Task<GetProductsResponse> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {        
       var products = await _productRepository.GetAllAsync(
           request.UserId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken)
            ?? throw new InvalidOperationException($"List of products is empty.");

        return new GetProductsResponse
        {
            Data = ProductMappers.ToDtoList(products.Items),
            TotalCount = products.TotalCount
        };
    }
}