using Microsoft.Extensions.DependencyInjection;


namespace CreateInvoiceSystem.Pdf.DI;

public static class PdfServiceCollectionExtensions
{
    public static IServiceCollection AddPdfModule(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddPdfModule(this IServiceCollection services, Action<IServiceCollection> registerImplementations)
    {
        registerImplementations?.Invoke(services);
        return services;
    }
}
