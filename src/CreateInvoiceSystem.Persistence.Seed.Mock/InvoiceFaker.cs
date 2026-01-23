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
    private static readonly string[] VatRates = { "23%", "8%", "5%", "zw" };

    private static Faker<InvoiceEntity> BaseFaker => new Faker<InvoiceEntity>("pl")
        .RuleFor(i => i.Title, f => $"Faktura {f.Random.Int(1000, 9999)}")
        .RuleFor(i => i.Comments, f => f.Lorem.Sentence())
        .RuleFor(i => i.CreatedDate, f => f.Date.Recent())
        .RuleFor(i => i.PaymentDate, f => f.Date.Soon(30))
        .RuleFor(i => i.MethodOfPayment, f => f.PickRandom(PaymentMethods))
        .RuleFor(i => i.TotalNet, _ => 0m)
        .RuleFor(i => i.TotalVat, _ => 0m)
        .RuleFor(i => i.TotalGross, _ => 0m);

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

        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.NewClient_ProductsFromDb:
                {
                    var client = ClientFaker.Generate(user);
                    invoice.ClientId = null;
                    invoice.ClientName = client.Name;
                    invoice.ClientNip = client.Nip;
                    var address = AddressFaker.Generate();
                    invoice.ClientAddress = FormatAddress(address);
                    break;
                }

            case InvoiceScenario.ExistingClient_NewProducts:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
                    invoice.ClientId = dbClient.ClientId;
                    invoice.ClientName = dbClient.Name;
                    invoice.ClientNip = dbClient.Nip;
                    invoice.ClientAddress = FormatAddress(dbClientAddress);
                    break;
                }
        }

        decimal totalNet = 0m;
        decimal totalVat = 0m;
        var faker = new Faker();
        List<ProductEntity> chosenProducts = new();

        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.ExistingClient_NewProducts:
                {
                    chosenProducts.Add(ProductFaker.Generate(user));
                    chosenProducts.Add(ProductFaker.Generate(user));
                    break;
                }

            case InvoiceScenario.NewClient_ProductsFromDb:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbProducts == null || dbProducts.Count < 2) throw new ArgumentException(nameof(dbProducts));
                    chosenProducts.AddRange(faker.PickRandom(dbProducts, 2));
                    break;
                }
        }

        foreach (var product in chosenProducts)
        {
            int quantity = RandomQuantity();
            decimal netPrice = product.Value ?? 0m;
            decimal lineNet = netPrice * quantity;
            string vatRate = faker.PickRandom(VatRates);

            totalNet += lineNet;
            totalVat += CalculateVat(lineNet, vatRate);
        }

        invoice.TotalNet = totalNet;
        invoice.TotalVat = totalVat;
        invoice.TotalGross = totalNet + totalVat;

        return invoice;
    }

    private static decimal CalculateVat(decimal net, string vatRate)
    {
        if (vatRate == "zw") return 0m;
        return decimal.TryParse(vatRate.Replace("%", ""), out var rate) ? net * (rate / 100) : 0m;
    }

    private static int RandomQuantity() => new Faker().Random.Int(1, 10);

    private static string? FormatAddress(AddressEntity? address) =>
        address == null ? null : $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";
}