using CreateInvoiceSystem.Modules.Users.Domain.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Modules.Users.Domain.DI;

public static class UserServiceCollectionExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddHostedService<UserCleanupService>();
        return services;
    }    
}
