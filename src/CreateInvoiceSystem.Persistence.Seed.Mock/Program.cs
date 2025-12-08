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
