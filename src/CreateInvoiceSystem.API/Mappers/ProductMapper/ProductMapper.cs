using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;

namespace CreateInvoiceSystem.API.Mappers.ProductMapper;

public static class ProductMapper
{
    public static ProductEntity ToEntity(Product p)
    {
        return new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Value = p.Value,
            UserId = p.UserId
        };
    }

    public static Product ToDomain(ProductEntity e)
    {
        if (e == null) return null;

        return new Product
        {
            ProductId = e.ProductId,
            Name = e.Name,
            Description = e.Description,
            Value = e.Value,
            UserId = e.UserId
        };
    }

    public static List<Product> ToDomainList(IEnumerable<ProductEntity> entities)
    {
        return entities?.Select(ToDomain).Where(p => p != null).ToList() ?? new List<Product>();
    }
}