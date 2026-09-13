using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class DeleteProductCommand : CommandBase<Product, ProductDto, IProductRepository>
{
    public override async Task<ProductDto> Execute(IProductRepository productRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var productEntity = await productRepository.GetByIdAsync(Parametr.ProductId, Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var productDto = ProductMappers.ToDto(productEntity);

        await productRepository.RemoveAsync(productEntity.ProductId, cancellationToken);        
        
        return productDto;            
    }
}