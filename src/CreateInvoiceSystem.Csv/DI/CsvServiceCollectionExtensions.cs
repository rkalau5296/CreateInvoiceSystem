using CreateInvoiceSystem.Csv.Interfaces;
using CreateInvoiceSystem.Csv.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Csv.DI;

public static class CsvServiceCollectionExtensions
{
    public static IServiceCollection AddCsvModule(this IServiceCollection services)
    {
        services.AddScoped<ICsvExportService, CsvExportService>();
        return services;
    }

    public static IServiceCollection AddCsvModule(this IServiceCollection services, System.Action<IServiceCollection> registerProviders)
    {
        services.AddScoped<ICsvExportService, CsvExportService>();
        registerProviders?.Invoke(services);
        return services;
    }
}
