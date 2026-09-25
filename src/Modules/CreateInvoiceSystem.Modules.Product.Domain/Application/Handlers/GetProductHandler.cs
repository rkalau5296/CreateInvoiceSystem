using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
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
