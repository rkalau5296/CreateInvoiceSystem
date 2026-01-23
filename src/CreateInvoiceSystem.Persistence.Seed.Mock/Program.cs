using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Persistence.Seed.Mock;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;

string? connectionString = null;
for (var i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (arg is "--connectionString" or "-cs")
    {
        if (i + 1 < args.Length) connectionString = args[i + 1];
    }
    if (arg.StartsWith("connectionString="))
    {
        var parts = arg.Split('=', 2);
        if (parts.Length == 2) connectionString = parts[1];
    }
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Brak argumentu connectionString. Uruchom aplikację z argumentem: connectionString=<wartość>");
    return;
}

var optionsBuilder = new DbContextOptionsBuilder<CreateInvoiceSystemDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var dbContext = new CreateInvoiceSystemDbContext(optionsBuilder.Options);

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
var faker = new Faker("pl");

Console.WriteLine("Seed project CreateInvoiceSystem with mock data.");
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Create Users");

var userAddresses = AddressFaker.Generate(1).ToList();
dbContext.Addresses.AddRange(userAddresses);
dbContext.SaveChanges();

var users = UserFaker.Generate(userAddresses.Select(a => a.AddressId)).ToList();
dbContext.Users.AddRange(users);
dbContext.SaveChanges();

foreach (var user in users)
{
    var userJson = JsonSerializer.Serialize(user, jsonOptions);
    Console.WriteLine(userJson);
}

Console.WriteLine("------------------------------------------------");
Console.WriteLine("Create Clients");

foreach (var user in users)
{
    var clientAddresses = AddressFaker.Generate(25).ToList();
    dbContext.Addresses.AddRange(clientAddresses);
    dbContext.SaveChanges();

    var clients = ClientFaker.Generate(25, user).ToList();
    for (int i = 0; i < clients.Count; i++)
        clients[i].AddressId = clientAddresses[i].AddressId;

    dbContext.Clients.AddRange(clients);
    dbContext.SaveChanges();
}

Console.WriteLine("------------------------------------------------");
Console.WriteLine("Create Products");

foreach (var user in users)
{
    var products = ProductFaker.Generate(25, user).ToList();
    dbContext.Products.AddRange(products);
    dbContext.SaveChanges();

    foreach (var product in products)
    {
        var productJson = JsonSerializer.Serialize(product, jsonOptions);
        Console.WriteLine(productJson);
    }
}

Console.WriteLine("------------------------------------------------");
Console.WriteLine("Create Invoices (and InvoicePositions via join entity)");

foreach (var user in users)
{
    var productsFromDb = dbContext.Products
        .Where(p => p.UserId == user.Id)
        .ToList();

    if (productsFromDb.Count < 2)
    {
        var bootstrapProducts = ProductFaker.Generate(5, user).ToList();
        dbContext.Products.AddRange(bootstrapProducts);
        dbContext.SaveChanges();
        productsFromDb = dbContext.Products.Where(p => p.UserId == user.Id).ToList();
    }

    var existingClient = dbContext.Clients.FirstOrDefault(c => c.UserId == user.Id);
    if (existingClient == null)
    {
        var seedAddr = AddressFaker.Generate();
        dbContext.Addresses.Add(seedAddr);
        dbContext.SaveChanges();

        var seedClient = ClientFaker.Generate(user);
        seedClient.AddressId = seedAddr.AddressId;
        dbContext.Clients.Add(seedClient);
        dbContext.SaveChanges();
        existingClient = seedClient;
    }
    var existingClientAddress = dbContext.Addresses.First(a => a.AddressId == existingClient.AddressId);

    var addr1 = AddressFaker.Generate();
    dbContext.Addresses.Add(addr1);
    dbContext.SaveChanges();

    var newClient1 = ClientFaker.Generate(user);
    newClient1.AddressId = addr1.AddressId;
    dbContext.Clients.Add(newClient1);
    dbContext.SaveChanges();

    var newProducts1 = ProductFaker.Generate(2, user).ToList();
    dbContext.Products.AddRange(newProducts1);
    dbContext.SaveChanges();

    var invoice1 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: newClient1,
        dbClientAddress: addr1,
        dbProducts: newProducts1
    );
    dbContext.Invoices.Add(invoice1);
    dbContext.SaveChanges();

    var inv1Positions = BuildPositionsFromProducts(invoice1.InvoiceId, newProducts1);
    dbContext.InvoicePositions.AddRange(inv1Positions);
    dbContext.SaveChanges();
    UpdateInvoiceTotals(dbContext, invoice1.InvoiceId);

    var addr2 = AddressFaker.Generate();
    dbContext.Addresses.Add(addr2);
    dbContext.SaveChanges();

    var newClient2 = ClientFaker.Generate(user);
    newClient2.AddressId = addr2.AddressId;
    dbContext.Clients.Add(newClient2);
    dbContext.SaveChanges();

    var invoice2 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.NewClient_ProductsFromDb,
        dbClient: null,
        dbClientAddress: null,
        dbProducts: productsFromDb
    );
    dbContext.Invoices.Add(invoice2);
    dbContext.SaveChanges();

    var inv2Products = PickTwo(productsFromDb);
    var inv2Positions = BuildPositionsFromProducts(invoice2.InvoiceId, inv2Products);
    dbContext.InvoicePositions.AddRange(inv2Positions);
    dbContext.SaveChanges();
    UpdateInvoiceTotals(dbContext, invoice2.InvoiceId);

    var newProducts3 = ProductFaker.Generate(2, user).ToList();
    dbContext.Products.AddRange(newProducts3);
    dbContext.SaveChanges();

    var invoice3 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: existingClient,
        dbClientAddress: existingClientAddress,
        dbProducts: newProducts3
    );
    dbContext.Invoices.Add(invoice3);
    dbContext.SaveChanges();

    var inv3Positions = BuildPositionsFromProducts(invoice3.InvoiceId, newProducts3);
    dbContext.InvoicePositions.AddRange(inv3Positions);
    dbContext.SaveChanges();
    UpdateInvoiceTotals(dbContext, invoice3.InvoiceId);

    var invoice4 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: existingClient,
        dbClientAddress: existingClientAddress,
        dbProducts: productsFromDb
    );
    dbContext.Invoices.Add(invoice4);
    dbContext.SaveChanges();

    var inv4Products = PickTwo(productsFromDb);
    var inv4Positions = BuildPositionsFromProducts(invoice4.InvoiceId, inv4Products);
    dbContext.InvoicePositions.AddRange(inv4Positions);
    dbContext.SaveChanges();
    UpdateInvoiceTotals(dbContext, invoice4.InvoiceId);

    var invoicesToPrint = new[] { invoice1, invoice2, invoice3, invoice4 };
    foreach (var inv in invoicesToPrint)
    {
        var positions = dbContext.InvoicePositions
            .Where(p => p.InvoiceId == inv.InvoiceId)
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.ProductDescription,
                p.ProductValue,
                p.Quantity,
                p.VatRate
            })
            .ToList();

        var invJson = JsonSerializer.Serialize(new
        {
            inv.InvoiceId,
            inv.Title,
            inv.TotalNet,
            inv.TotalVat,
            inv.TotalGross,
            Client = new { inv.ClientName, inv.ClientNip, inv.ClientAddress },
            Positions = positions
        }, jsonOptions);

        Console.WriteLine(invJson);
    }
}

static List<InvoicePositionEntity> BuildPositionsFromProducts(int invoiceId, IReadOnlyList<ProductEntity> products)
{
    var f = new Faker();
    var vatRates = new[] { "23%", "8%", "5%", "zw" };
    var positions = new List<InvoicePositionEntity>();
    foreach (var product in products)
    {
        positions.Add(new InvoicePositionEntity
        {
            InvoiceId = invoiceId,
            ProductId = product.ProductId,
            ProductName = product.Name,
            ProductDescription = product.Description,
            ProductValue = product.Value,
            Quantity = f.Random.Int(1, 10),
            VatRate = f.PickRandom(vatRates)
        });
    }
    return positions;
}

static IReadOnlyList<ProductEntity> PickTwo(List<ProductEntity> products)
{
    var chosen = new Faker().PickRandom(products, 2).ToArray();
    return new List<ProductEntity> { chosen[0], chosen[1] };
}

static void UpdateInvoiceTotals(CreateInvoiceSystemDbContext ctx, int invoiceId)
{
    var positions = ctx.InvoicePositions
        .Where(p => p.InvoiceId == invoiceId)
        .ToList();

    decimal totalNet = 0;
    decimal totalVat = 0;

    foreach (var p in positions)
    {
        var net = (p.ProductValue ?? 0m) * p.Quantity;
        totalNet += net;
        totalVat += CalculateVatValue(net, p.VatRate);
    }

    var invoice = ctx.Invoices.First(i => i.InvoiceId == invoiceId);
    invoice.TotalNet = totalNet;
    invoice.TotalVat = totalVat;
    invoice.TotalGross = totalNet + totalVat;

    ctx.Invoices.Update(invoice);
    ctx.SaveChanges();
}

static decimal CalculateVatValue(decimal net, string vatRate)
{
    if (vatRate == "zw") return 0m;
    return decimal.TryParse(vatRate.Replace("%", ""), out var rate) ? net * (rate / 100) : 0m;
}