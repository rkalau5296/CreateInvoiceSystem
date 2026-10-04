using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.Modules.Clients.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using CreateInvoiceSystem.Modules.Products.Interfaces;
using CreateInvoiceSystem.Modules.Users.Interfaces;

namespace CreateInvoiceSystem.API.DI;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddApiRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
