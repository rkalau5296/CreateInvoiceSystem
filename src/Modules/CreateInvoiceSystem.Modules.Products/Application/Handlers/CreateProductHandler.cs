using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Interfaces;
using CreateInvoiceSystem.Modules.Products.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;
public class CreateProductHandler(IProductRepository _productRepository) : IRequestHandler<CreateProductRequest, CreateProductResponse>
{   
    public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {  
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