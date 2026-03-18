using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Clients.Persistence.Persistence;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Products.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Users.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Persistence.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CreateInvoiceSystemDbContext>(db =>
            db.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IAddressDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IClientDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IInvoicePosistionDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IInvoiceDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IUserDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());

        return services;
    }
}
