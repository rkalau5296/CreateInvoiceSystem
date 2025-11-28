namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class ProductMappers
{
    public static ProductDto ToDto(this Product product) =>
        new(product.ProductId, product.Name, product.Description, product.Value, product.UserId, product.User.ToDto(), product.InvoicePositions?.Select(ip => ip.ToDto()));

    public static Product ToEntity(this ProductDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId,
            User = dto.UserDto.ToEntity(),
            InvoicePositions = dto.InvoicePositions?.Select(ipDto => ipDto.ToEntity()).ToList()
        };

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> products) =>
         [.. products.Select(a => a.ToDto())];
}
