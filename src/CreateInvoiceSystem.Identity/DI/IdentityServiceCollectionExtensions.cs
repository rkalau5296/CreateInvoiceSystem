using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Identity.DI;

public static class IdentityServiceCollectionExtensions
{   
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtProvider, JwtProvider>();       

        return services;
    }
}
