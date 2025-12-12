using Bogus;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Users.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class ClientFaker
{
    private static Faker<Client> Faker => new Faker<Client>()
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Nip, f => f.Random.ReplaceNumbers("##########"))
        .RuleFor(c => c.Address, f => AddressFaker.Generate())
        .RuleFor(c => c.IsDeleted, f => false)
        .FinishWith((f, u) => u.AddressId = u.Address.AddressId)
    ;
    
    public static Client Generate(User user)
    {
        var client = Faker.Generate();
        //client.User = user;
        return client;
    }
    
    public static IEnumerable<Client> Generate(int count, User user) => 
        Enumerable.Range(0, count).Select(_ => Generate(user)).ToList();
}
