
using Bogus;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class UserFaker
{
    private static Faker<UserEntity> BaseFaker => new Faker<UserEntity>("pl")
        .RuleFor(u => u.Name, f => $"{f.Name.FirstName()} {f.Name.LastName()}")
        .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))        
        .RuleFor(u => u.Nip, f => f.Random.ReplaceNumbers("##########"));    
    public static UserEntity Generate(int addressId)
    {
        var faker = BaseFaker.Clone().FinishWith((f, u) => u.AddressId = addressId);
        return faker.Generate();
    }
    
    public static IEnumerable<UserEntity> Generate(int count, Func<int> addressIdFactory)
    {
        if (addressIdFactory is null) throw new ArgumentNullException(nameof(addressIdFactory));
        var faker = BaseFaker.Clone().FinishWith((f, u) => u.AddressId = addressIdFactory());
        return faker.Generate(count);
    }
    
    public static IEnumerable<UserEntity> Generate(IEnumerable<int> addressIds)
    {
        if (addressIds is null) throw new ArgumentNullException(nameof(addressIds));
        foreach (var id in addressIds)
            yield return Generate(id);
    }
}