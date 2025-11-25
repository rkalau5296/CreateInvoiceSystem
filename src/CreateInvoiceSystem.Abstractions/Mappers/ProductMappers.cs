namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ProductMappers
{
    public static ProductDto ToDto(this Product Product) =>
        new(Product.ProductId, Product.Name, Product.Value);

    public static Product ToEntity(this ProductDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Value = dto.Value            
        };

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> addresses) =>
         [.. addresses.Select(a => a.ToDto())];
}
