using Bogus;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class AddressFaker
{
    private static Faker<AddressEntity> Faker => new Faker<AddressEntity>()
        .RuleFor(a => a.Street, f => f.Address.StreetName())
        .RuleFor(a => a.Number, f => f.Address.BuildingNumber())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.PostalCode, f => f.Random.ReplaceNumbers("##-###"))
        .RuleFor(a => a.Country, f => f.Address.Country())
    ;

    public static AddressEntity Generate() => Faker.Generate();
    public static IEnumerable<AddressEntity> Generate(int count) => Faker.Generate(count);
}
