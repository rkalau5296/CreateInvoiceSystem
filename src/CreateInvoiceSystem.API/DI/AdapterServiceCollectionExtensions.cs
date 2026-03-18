using CreateInvoiceSystem.API.Adapters.CsvDataAdapter;
using CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;
using CreateInvoiceSystem.API.Adapters.PdfAdapter;
using CreateInvoiceSystem.API.Adapters.UserAuthAdapter;
using CreateInvoiceSystem.API.Adapters.UserEmailAdapter;
using CreateInvoiceSystem.API.Adapters.UserTokenAdapter;
using CreateInvoiceSystem.Csv.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.DI;

public static class AdapterServiceCollectionExtensions
{
    public static IServiceCollection AddApiAdapters(this IServiceCollection services)
    {     
        services.AddScoped<IUserTokenService, UserTokenAdapter>();
        services.AddScoped<IUserAuthService, UserAuthServiceAdapter>();
        services.AddTransient<IUserEmailSender, UserEmailAdapter>();
        services.AddTransient<IInvoiceEmailSender, InvoiceEmailAdapter>();
        services.AddScoped<IExportDataProvider, InvoiceExportDataProvider>();
        services.AddScoped<IInvoiceExportService, InvoiceToPdfAdapter>();

        return services;
    }
}
