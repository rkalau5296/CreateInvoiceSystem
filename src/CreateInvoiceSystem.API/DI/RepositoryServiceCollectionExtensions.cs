using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

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
