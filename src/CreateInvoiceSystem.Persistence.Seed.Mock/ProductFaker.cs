using Bogus;

using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Users.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class ProductFaker
{
    private static Faker<Product> Faker => new Faker<Product>()
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Value, f => Math.Round(f.Random.Decimal(1m, 5000m), 2))
        .RuleFor(p => p.IsDeleted, f => false);

    public static Product Generate(User user)
    {
        var product = Faker.Generate();        
        product.UserId = user.UserId;       

        return product;
    }

    public static IEnumerable<Product> Generate(int count, User user) =>
        Enumerable.Range(0, count).Select(_ => Generate(user)).ToList();
}