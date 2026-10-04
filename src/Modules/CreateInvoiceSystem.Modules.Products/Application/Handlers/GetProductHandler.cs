using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Interfaces;
using CreateInvoiceSystem.Modules.Products.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;
public class GetProductHandler(IProductRepository _productRepository) : IRequestHandler<GetProductRequest, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, request.UserId, cancellationToken: cancellationToken)
             ?? throw new InvalidOperationException($"Product with ID {request.Id} not found.");

        return new GetProductResponse
        {
            Data = ProductMappers.ToDto(product),
        };
    }
}
