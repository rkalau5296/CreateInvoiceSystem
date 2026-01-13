using Bogus;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public static class ClientFaker
{
    private static Faker<ClientEntity> Faker => new Faker<ClientEntity>()
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Nip, f => f.Random.ReplaceNumbers("##########"))        
        .RuleFor(c => c.IsDeleted, f => false)        
    ;

    public static ClientEntity Generate(UserEntity user)
    {
        var client = Faker.Generate();
        client.UserId = user.Id;
        return client;
    }
    
    public static IEnumerable<ClientEntity> Generate(int count, UserEntity user) => 
        Enumerable.Range(0, count).Select(_ => Generate(user)).ToList();
}
