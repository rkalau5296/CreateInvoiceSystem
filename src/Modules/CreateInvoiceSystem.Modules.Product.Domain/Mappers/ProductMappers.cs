using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Products.Domain.Mappers;
public static class ProductMappers
{
    public static ProductDto ToDto(this Product product) =>
        product == null
        ? throw new ArgumentNullException(nameof(product), "Product cannot be null when mapping to ProductDto.")
        :
        new(product.ProductId,
        product.Name,
        product.Description,
        product.Value,
        product.UserId,
        product.IsDeleted
        );

    public static Product ToEntity(this ProductDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Product cannot be null when mapping to Product.")
        :
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId            
        };
    public static CreateProductDto ToCreateDto(this Product product) =>
        product == null
        ? throw new ArgumentNullException(nameof(product), "Product cannot be null when mapping to CreateProductDto.")
        :
        new(
        product.Name,
        product.Description,
        product.Value,
        product.UserId
        );

    public static Product ToEntity(this CreateProductDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "Product cannot be null when mapping to Product.")
        :
        new()
        {
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId
        };

    public static UpdateProductDto ToUpdatedDto(this Product product) =>
    product == null
        ? throw new ArgumentNullException(nameof(product), "Product cannot be null when mapping to UpdateProductDto.")
        : new UpdateProductDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.Value,
            product.UserId,
            product.IsDeleted
        );

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products) =>
        products == null
        ? throw new ArgumentNullException(nameof(products), "Products cannot be null when mapping to ProductDto.")
        :
         [.. products.Select(a => a.ToDto())];
}
