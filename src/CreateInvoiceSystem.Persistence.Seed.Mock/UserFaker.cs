
using Bogus;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class UserFaker
{
    private static Faker<UserEntity> BaseFaker => new Faker<UserEntity>("pl")
        .RuleFor(u => u.Name, f => $"{f.Name.FirstName()} {f.Name.LastName()}")
        .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
        .RuleFor(u => u.Password, f => f.Internet.Password())
        .RuleFor(u => u.Nip, f => f.Random.ReplaceNumbers("##########"));

    // Generuje użytkownika z podanym AddressId (bez nawigacji Address)
    public static UserEntity Generate(int addressId)
    {
        var faker = BaseFaker.Clone().FinishWith((f, u) => u.AddressId = addressId);
        return faker.Generate();
    }

    // Generuje wielu użytkowników, pobierając AddressId z fabryki (np. po zapisaniu Address do DB)
    public static IEnumerable<UserEntity> Generate(int count, Func<int> addressIdFactory)
    {
        if (addressIdFactory is null) throw new ArgumentNullException(nameof(addressIdFactory));
        var faker = BaseFaker.Clone().FinishWith((f, u) => u.AddressId = addressIdFactory());
        return faker.Generate(count);
    }

    // Generuje wielu użytkowników na podstawie gotowej listy AddressId (1:1)
    public static IEnumerable<UserEntity> Generate(IEnumerable<int> addressIds)
    {
        if (addressIds is null) throw new ArgumentNullException(nameof(addressIds));
        foreach (var id in addressIds)
            yield return Generate(id);
    }
}