namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ProductMappers
{
    public static ProductDto ToDto(this Product product) =>
        new(product.ProductId,
        product.Name,
        product.Description,
        product.Value,
        product.UserId
        );

    public static Product ToEntity(this ProductDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId            
        };
    public static CreateProductDto ToCreateDto(this Product product) =>
        new(
        product.Name,
        product.Description,
        product.Value,
        product.UserId
        );

    public static Product ToEntity(this CreateProductDto dto) =>
        new()
        {
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId
        };

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products) =>
         [.. products.Select(a => a.ToDto())];
}
