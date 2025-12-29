using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class CreateProductCommand : CommandBase<CreateProductDto, CreateProductDto, IProductRepository>
{
    public override async Task<CreateProductDto> Execute(IProductRepository _productRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(_productRepository));

        var exists = await _productRepository.ExistsAsync(
           name: Parametr.Name,
           userId: Parametr.UserId,
           cancellationToken: cancellationToken);

        if (exists)
            throw new InvalidOperationException("The product with the same name already exists.");

        var entity = ProductMappers.ToEntity(Parametr);

        await _productRepository.AddAsync(entity, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _productRepository.GetByIdAsync(entity.ProductId, cancellationToken);

        return persisted is not null
            ? ProductMappers.ToCreateDto(persisted)
            : throw new InvalidOperationException("Product was saved but could not be reloaded.");
    }
}
