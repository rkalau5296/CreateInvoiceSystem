using Bogus;
using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class UserFaker
{
    private static Faker<User> Faker => new Faker<User>()
        .RuleFor(u => u.Name, f => $"{f.Name.FirstName()} {f.Name.LastName()}")
        .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
        .RuleFor(u => u.Password, f => f.Internet.Password())
        .RuleFor(u => u.Nip, f => f.Random.ReplaceNumbers("##########"))
        .RuleFor(u => u.Address, f => AddressFaker.Generate())
        .FinishWith((f, u) => u.AddressId = u.Address.AddressId)
    ;
    
    public static User Generate() => Faker.Generate();
    public static IEnumerable<User> Generate(int count) => Faker.Generate(count);
}
