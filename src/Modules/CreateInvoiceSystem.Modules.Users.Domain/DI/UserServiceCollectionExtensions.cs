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

    public static IServiceCollection AddUserModule(this IServiceCollection services, Action<IServiceCollection> registerImplementations)
    {
        services.AddHostedService<UserCleanupService>();
        registerImplementations?.Invoke(services);
        return services;
    }
}
