using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class UpdateProductCommand : CommandBase<UpdateProductDto, UpdateProductDto, IProductRepository>
{
    public override async Task<UpdateProductDto> Execute(IProductRepository _productRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(Parametr));

        var product = await _productRepository.GetByIdAsync(Parametr.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var oldName = product.Name;
        var oldDescription = product.Description;
        var oldValue = product.Value;

        product.Name = Parametr.Name ?? product.Name;
        product.Description = Parametr.Description ?? product.Description;
        product.Value = Parametr.Value ?? product.Value;

        var updatedProduct = await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _productRepository.GetByIdAsync(updatedProduct.ProductId, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldDescription, persisted.Description, StringComparison.Ordinal) ||
            oldValue != persisted.Value
        );

        return hasChanged
            ? ProductMappers.ToUpdatedDto(persisted)
            : throw new InvalidOperationException($"No new data for product with ID {Parametr.ProductId}.");
    }
}
