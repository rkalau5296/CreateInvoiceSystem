using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
public class CreateProductCommand : CommandBase<CreateProductDto, CreateProductDto, IProductRepository>
{
    public override async Task<CreateProductDto> Execute(IProductRepository productRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var exists = await productRepository.ExistsAsync(Parametr.Name, Parametr.UserId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Istnieje już produkt o takiej nazwie.");

        var entity = ProductMappers.ToEntity(Parametr);

        var savedProduct = await productRepository.AddAsync(entity, cancellationToken);

        return ProductMappers.ToCreateDto(savedProduct);            
    }
}
