using Bogus;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Users.Entities;

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

    private static Faker<Invoice> BaseFaker => new Faker<Invoice>("pl")
        .RuleFor(i => i.Title, f => $"Faktura {f.Random.Int(1000, 9999)}")
        .RuleFor(i => i.Comments, f => f.Lorem.Sentence())
        .RuleFor(i => i.CreatedDate, f => f.Date.Recent())
        .RuleFor(i => i.PaymentDate, f => f.Date.Soon(30))
        .RuleFor(i => i.MethodOfPayment, f => f.PickRandom(PaymentMethods))
        .RuleFor(i => i.TotalAmount, _ => 0m); 

    public static Invoice Generate(
        User user,
        InvoiceScenario scenario,
        Client? dbClient = null,
        IReadOnlyList<Product>? dbProducts = null)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var invoice = BaseFaker.Generate();
        invoice.UserId = user.UserId;
        invoice.InvoicePositions = new List<InvoicePosition>();

        // Klient
        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.NewClient_ProductsFromDb:
                {
                    var client = ClientFaker.Generate(user);
                    invoice.Client = client;
                    invoice.ClientId = null;
                    invoice.ClientName = client.Name;
                    invoice.ClientNip = client.Nip;
                    invoice.ClientAddress = FormatAddress(client.Address);
                    break;
                }

            case InvoiceScenario.ExistingClient_NewProducts:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbClient == null)
                        throw new ArgumentNullException(nameof(dbClient), "Wariant wymaga istniejącego klienta z bazy.");

                    invoice.Client = dbClient;
                    invoice.ClientId = dbClient.ClientId;
                    invoice.ClientName = dbClient.Name;
                    invoice.ClientNip = dbClient.Nip;
                    invoice.ClientAddress = FormatAddress(dbClient.Address);
                    break;
                }
        }

        // Produkty / pozycje
        switch (scenario)
        {
            case InvoiceScenario.NewClient_NewProducts:
            case InvoiceScenario.ExistingClient_NewProducts:
                {
                    // dwa produkty ad-hoc ("z palca") — bez FK do Product
                    var p1 = ProductFaker.Generate(user);
                    var p2 = ProductFaker.Generate(user);

                    invoice.InvoicePositions.Add(CreatePositionFromAdHoc(p1, qty: RandomQuantity()));
                    invoice.InvoicePositions.Add(CreatePositionFromAdHoc(p2, qty: RandomQuantity()));
                    break;
                }

            case InvoiceScenario.NewClient_ProductsFromDb:
            case InvoiceScenario.ExistingClient_ProductsFromDb:
                {
                    if (dbProducts == null || dbProducts.Count < 2)
                        throw new ArgumentException("Wariant wymaga co najmniej dwóch produktów z bazy.", nameof(dbProducts));

                    var chosen = new Faker().PickRandom(dbProducts, 2).ToArray();
                    invoice.InvoicePositions.Add(CreatePositionFromProduct(chosen[0], qty: RandomQuantity()));
                    invoice.InvoicePositions.Add(CreatePositionFromProduct(chosen[1], qty: RandomQuantity()));
                    break;
                }
        }

        // Suma z pozycji
        invoice.TotalAmount = invoice.InvoicePositions
            .Sum(pos => (pos.ProductValue ?? 0m) * pos.Quantity);

        return invoice;
    }

    private static InvoicePosition CreatePositionFromProduct(Product product, int qty) =>
        new()
        {
            ProductId = product.ProductId,
            Product = product,
            ProductName = product.Name,
            ProductDescription = product.Description,
            ProductValue = product.Value,
            Quantity = qty
        };

    private static InvoicePosition CreatePositionFromAdHoc(Product sourceLikeProduct, int qty) =>
        new()
        {
            ProductId = null,       // brak powiązania FK
            Product = null!,        // nawigacja nieużywana w tym wariancie
            ProductName = sourceLikeProduct.Name,
            ProductDescription = sourceLikeProduct.Description,
            ProductValue = sourceLikeProduct.Value,
            Quantity = qty
        };

    private static int RandomQuantity() => new Faker().Random.Int(1, 10);

    private static string? FormatAddress(Address address) =>
        address == null ? null :
        $"{address.Street} {address.Number}, {address.City}, {address.PostalCode}, {address.Country}";
}