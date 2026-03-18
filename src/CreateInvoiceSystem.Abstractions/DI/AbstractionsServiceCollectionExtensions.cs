using CreateInvoiceSystem.Abstractions.Executors;
using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Abstractions.DI;

public static class AbstractionsServiceCollectionExtensions
{
    public static IServiceCollection AddAbstractionsModule(this IServiceCollection services)
    {        
        services.AddTransient<ICommandExecutor, CommandExecutor>();
        services.AddTransient<IQueryExecutor, QueryExecutor>();
        return services;
    }
}

