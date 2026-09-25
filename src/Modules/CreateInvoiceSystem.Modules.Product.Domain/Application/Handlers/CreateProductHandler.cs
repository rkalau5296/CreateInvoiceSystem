using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
public class CreateProductHandler(IProductRepository _productRepository) : IRequestHandler<CreateProductRequest, CreateProductResponse>
{   
    public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Product);

        var exists = await _productRepository.ExistsAsync(request.Product.Name, request.Product.UserId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Istnieje już produkt o takiej nazwie.");

        var entity = ProductMappers.ToEntity(request.Product);

        var savedProduct = await _productRepository.AddAsync(entity, cancellationToken);

        return new CreateProductResponse()
        {
            Data = ProductMappers.ToCreateDto(savedProduct)
        };
    }
}