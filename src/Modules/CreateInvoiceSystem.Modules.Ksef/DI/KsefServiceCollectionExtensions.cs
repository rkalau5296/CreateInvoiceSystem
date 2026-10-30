using CreateInvoiceSystem.Modules.Ksef.Application.Options;
using CreateInvoiceSystem.Modules.Ksef.Application.Services;
using KSeF.Client.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Modules.Ksef.DI;

public static class KsefServiceCollectionExtensions
{
    public static IServiceCollection AddKsefModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KsefApiOptions>(configuration.GetSection("KsefApi"));

        KsefApiOptions ksefApiOptions = configuration.GetSection("KsefApi").Get<KsefApiOptions>() ?? new KsefApiOptions();

        services.AddKSeFClient(options =>
        {
            options.BaseUrl = ksefApiOptions.BaseUrl;
        });

        services.AddCryptographyClient();
        services.AddHostedService<KsefSendingBackgroundService>();

        return services;
    }
}