using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class DeleteProductCommand : CommandBase<Product, ProductDto, IProductRepository>
{
    public override async Task<ProductDto> Execute(IProductRepository _productRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(Parametr));

        var productEntity = await _productRepository.GetByIdAsync(Parametr.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var productDto = ProductMappers.ToDto(productEntity);

        await _productRepository.RemoveAsync(productEntity.ProductId, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var prodExists = await _productRepository.ExistsByIdAsync(productEntity.ProductId, cancellationToken);

        return !prodExists
            ? productDto
            : throw new InvalidOperationException($"Failed to delete Product with ID {Parametr.ProductId}.");
    }
}