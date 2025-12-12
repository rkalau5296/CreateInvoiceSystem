using Bogus;
using CreateInvoiceSystem.Modules.Addresses.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class AddressFaker
{
    private static Faker<Address> Faker => new Faker<Address>()
        .RuleFor(a => a.Street, f => f.Address.StreetName())
        .RuleFor(a => a.Number, f => f.Address.BuildingNumber())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.PostalCode, f => f.Random.ReplaceNumbers("##-###"))
        .RuleFor(a => a.Country, f => f.Address.Country())
    ;
    
    public static Address Generate() => Faker.Generate();
    public static IEnumerable<Address> Generate(int count) => Faker.Generate(count);
}
