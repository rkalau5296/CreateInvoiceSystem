using CreateInvoiceSystem.Modules.Users.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Modules.Users.DI;

public static class UserServiceCollectionExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddHostedService<UserCleanupService>();
        return services;
    }    
}
