using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Interfaces;
using CreateInvoiceSystem.Modules.Products.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

public class UpdateProductHandler(IProductRepository _productRepository) : IRequestHandler<UpdateProductRequest, UpdateProductResponse>
{    
    public async Task<UpdateProductResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found.");

        product.Name = request.Product.Name ?? product.Name;
        product.Description = request.Product.Description ?? product.Description;
        product.Value = request.Product.Value ?? product.Value;

        var updatedProduct = await _productRepository.UpdateAsync(product, cancellationToken);

        return new UpdateProductResponse()
        {
            Data = ProductMappers.ToUpdatedDto(updatedProduct)
        };
    }
}
