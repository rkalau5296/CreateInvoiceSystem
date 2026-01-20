using Bogus;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class ProductFaker
{
    private static Faker<ProductEntity> Faker => new Faker<ProductEntity>()
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Value, f => Math.Round(f.Random.Decimal(1m, 5000m), 2))
        .RuleFor(p => p.IsDeleted, f => false);

    public static ProductEntity Generate(UserEntity user)
    {
        var product = Faker.Generate();        
        product.UserId = user.Id;       

        return product;
    }

    public static IEnumerable<ProductEntity> Generate(int count, UserEntity user) =>
        Enumerable.Range(0, count).Select(_ => Generate(user)).ToList();
}