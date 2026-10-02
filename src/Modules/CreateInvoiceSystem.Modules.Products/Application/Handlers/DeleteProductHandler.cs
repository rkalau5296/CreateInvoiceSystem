using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;

public class DeleteProductHandler(IProductRepository _productRepository) : IRequestHandler<DeleteProductRequest, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var productEntity = await _productRepository.GetByIdAsync(request.Id, request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found.");        

        await _productRepository.RemoveAsync(productEntity.ProductId, cancellationToken);      

        return new DeleteProductResponse()
        {
            Data = ProductMappers.ToDto(productEntity)
        };
    }
}

