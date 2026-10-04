using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace CreateInvoiceSystem.Modules.Nbp.DI;

public static class NbpServiceCollectionExtensions
{
    public static IServiceCollection AddNbpModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NbpApiOptions>(configuration.GetSection("NbpApi"));        
        return services;
    }   
}
