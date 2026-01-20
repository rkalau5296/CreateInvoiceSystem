using CreateInvoiceSystem.Pdf.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace CreateInvoiceSystem.Pdf.Extensions;

public static class Extensions
{
    public static IServiceCollection AddPdfModule(this IServiceCollection services)
    {        
        QuestPDF.Settings.License = LicenseType.Community;
     
        services.AddScoped<IPdfGenerator, QuestPdfGenerator>();

        return services;
    }
}