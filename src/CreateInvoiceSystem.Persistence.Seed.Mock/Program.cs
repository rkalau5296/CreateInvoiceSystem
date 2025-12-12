using System.Text.Json;
using CreateInvoiceSystem.Persistence.Seed.Mock;
using Microsoft.EntityFrameworkCore;
using CreateInvoiceSystem.Persistence;

string? connectionString = null;
for (var i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (arg is "--connectionString" or "-cs")
    {
        connectionString = args[i + 1];
    }
    if (arg.StartsWith("connectionString="))
    {
        connectionString = arg.Split('=')[1];
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

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

Console.WriteLine("Seed project CreateInvoiceSystem with mock data.");
Console.WriteLine("------------------------------------------------");
Console.WriteLine("Create Users");
var users = UserFaker.Generate(1).ToList();
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
    var clients = ClientFaker.Generate(25, user).ToList();
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
Console.WriteLine("Create Invoices");

foreach (var user in users)
{    
    var productsFromDb = dbContext.Products
        .Where(p => p.UserId == user.UserId)
        .ToList();

    if (productsFromDb.Count < 2)
    {
        var bootstrapProducts = ProductFaker.Generate(5, user).ToList();
        dbContext.Products.AddRange(bootstrapProducts);
        dbContext.SaveChanges();
        productsFromDb = dbContext.Products.Where(p => p.UserId == user.UserId).ToList();
    }
    
    var existingClient = dbContext.Clients.FirstOrDefault(c => c.UserId == user.UserId);
    if (existingClient == null)
    {
        var seedClient = ClientFaker.Generate(user);
        dbContext.Clients.Add(seedClient);
        dbContext.SaveChanges();
        existingClient = seedClient;
    }
    
    var newClient1 = ClientFaker.Generate(user);
    dbContext.Clients.Add(newClient1);
    dbContext.SaveChanges();

    var newProducts1 = ProductFaker.Generate(2, user).ToList();
    dbContext.Products.AddRange(newProducts1);
    dbContext.SaveChanges();

    var invoice1 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: newClient1,
        dbProducts: newProducts1
    );
    dbContext.Invoices.Add(invoice1);
    dbContext.SaveChanges();
    
    var newClient2 = ClientFaker.Generate(user);
    dbContext.Clients.Add(newClient2);
    dbContext.SaveChanges();

    var invoice2 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.NewClient_ProductsFromDb,
        dbClient: null,
        dbProducts: productsFromDb
    );
    dbContext.Invoices.Add(invoice2);
    dbContext.SaveChanges();
    
    var newProducts3 = ProductFaker.Generate(2, user).ToList();
    dbContext.Products.AddRange(newProducts3);
    dbContext.SaveChanges();

    var invoice3 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: existingClient,
        dbProducts: newProducts3
    );
    dbContext.Invoices.Add(invoice3);
    dbContext.SaveChanges();

    
    var invoice4 = InvoiceFaker.Generate(
        user,
        InvoiceScenario.ExistingClient_ProductsFromDb,
        dbClient: existingClient,
        dbProducts: productsFromDb
    );
    dbContext.Invoices.Add(invoice4);
    dbContext.SaveChanges();
    
    var invoicesToPrint = new[] { invoice1, invoice2, invoice3, invoice4 };
    foreach (var inv in invoicesToPrint)
    {
        var invJson = JsonSerializer.Serialize(new
        {
            inv.InvoiceId,
            inv.Title,
            inv.TotalAmount,
            Client = new { inv.ClientName, inv.ClientNip, inv.ClientAddress },
            Positions = inv.InvoicePositions.Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.ProductDescription,
                p.ProductValue,
                p.Quantity
            })
        }, jsonOptions);

        Console.WriteLine(invJson);
    }
}