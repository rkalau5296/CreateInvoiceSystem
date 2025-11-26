namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ProductMappers
{
    public static ProductDto ToDto(this Product Product) =>
        new(Product.ProductId, Product.Name, Product.Value, Product.UserId, Product.User.ToDto());

    public static Product ToEntity(this ProductDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Value = dto.Value,
            UserId = dto.UserId,
            User = dto.UserDto.ToEntity()
        };

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> addresses) =>
         [.. addresses.Select(a => a.ToDto())];
}
