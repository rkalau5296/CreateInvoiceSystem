using Bogus;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Persistence.Seed.Mock;

public enum InvoiceScenario
{
    NewClient_NewProducts,
    NewClient_ProductsFromDb,
    ExistingClient_NewProducts,
    ExistingClient_ProductsFromDb
}

public static class InvoiceFaker
{
    private static readonly string[] PaymentMethods = { "Przelew", "Gotówka", "BLIK", "Karta" };

    private static Faker<InvoiceEntity> BaseFaker => new Faker<InvoiceEntity>("pl")
        .RuleFor(i => i.Title, f => $"Faktura {f.Random.Int(1000, 9999)}")
        .RuleFor(i => i.Comments, f => f.Lorem.Sentence())
        .RuleFor(i => i.CreatedDate, f => f.Date.Recent())
        .RuleFor(i => i.PaymentDate, f => f.Date.Soon(30))
        .RuleFor(i => i.MethodOfPayment, f => f.PickRandom(PaymentMethods))
        .RuleFor(i => i.TotalAmount, _ => 0m);

    // Prosty faker adresu na potrzeby snapshotu na fakturze
    private static Faker<AddressEntity> AddressFaker => new Faker<AddressEntity>("pl")
        .RuleFor(a => a.Street, f => f.Address.StreetName())
        .RuleFor(a => a.Number, f => f.Address.BuildingNumber())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
        .RuleFor(a => a.Country, f => "Polska");

    public static InvoiceEntity Generate(
        UserEntity user,
        InvoiceScenario scenario,
        ClientEntity? dbClient = null,
        AddressEntity? dbClientAddress = null,
        IReadOnlyList<ProductEntity>? dbProducts = null)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var invoice = BaseFaker.Generate();
        invoice.UserId = user.Id;

        // Klient (snapshot danych klienta na fakturze)
        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.NewClient_ProductsFromDb:
                {
                    var client = ClientFaker.Generate(user);

                    invoice.ClientId = null;
                    invoice.ClientName = client.Name;
                    invoice.ClientNip = client.Nip;

                    // ClientEntity nie ma nawigacji Address, więc generujemy adres niezależnie dla snapshotu
                    var address = AddressFaker.Generate();
                    invoice.ClientAddress = FormatAddress(address);
                    break;
                }

            case InvoiceScenario.ExistingClient_NewProducts:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbClient == null)
                        throw new ArgumentNullException(nameof(dbClient), "Wariant wymaga istniejącego klienta z bazy.");

                    invoice.ClientId = dbClient.ClientId;
                    invoice.ClientName = dbClient.Name;
                    invoice.ClientNip = dbClient.Nip;

                    // Snapshot adresu z bazy — przekaż AddressEntity osobno (brak nawigacji w ClientEntity)
                    invoice.ClientAddress = FormatAddress(dbClientAddress);
                    break;
                }
        }

        // Produkty → wyliczenie sumy (InvoiceEntity nie ma pozycji, więc tylko suma)
        decimal total = 0m;
        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.ExistingClient_NewProducts:
                {
                    var p1 = ProductFaker.Generate(user);
                    var p2 = ProductFaker.Generate(user);

                    total += (p1.Value ?? 0m) * RandomQuantity();
                    total += (p2.Value ?? 0m) * RandomQuantity();
                    break;
                }

            case InvoiceScenario.NewClient_ProductsFromDb:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbProducts == null || dbProducts.Count < 2)
                        throw new ArgumentException("Wariant wymaga co najmniej dwóch produktów z bazy.", nameof(dbProducts));

                    var chosen = new Faker().PickRandom(dbProducts, 2).ToArray();
                    total += (chosen[0].Value ?? 0m) * RandomQuantity();
                    total += (chosen[1].Value ?? 0m) * RandomQuantity();
                    break;
                }
        }

        invoice.TotalAmount = total;
        return invoice;
    }

    private static int RandomQuantity() => new Faker().Random.Int(1, 10);

    private static string? FormatAddress(AddressEntity? address) =>
        address == null ? null :
        $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";
}