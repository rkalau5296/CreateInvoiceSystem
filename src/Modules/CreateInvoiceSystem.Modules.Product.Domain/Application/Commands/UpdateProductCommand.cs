using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class UpdateProductCommand : CommandBase<UpdateProductDto, UpdateProductDto, IProductRepository>
{
    public override async Task<UpdateProductDto> Execute(IProductRepository productRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var product = await productRepository.GetByIdAsync(Parametr.ProductId, Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");               

        product.Name = Parametr.Name ?? product.Name;
        product.Description = Parametr.Description ?? product.Description;
        product.Value = Parametr.Value ?? product.Value;

        var updatedProduct = await productRepository.UpdateAsync(product, cancellationToken);

        return ProductMappers.ToUpdatedDto(updatedProduct);            
    }
}
