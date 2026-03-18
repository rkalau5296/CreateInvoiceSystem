using Microsoft.Extensions.DependencyInjection;

namespace CreateInvoiceSystem.Mail.DI
{
    public static class MailServiceCollectionExtensions
    {
        public static IServiceCollection AddMailModule(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, SmtpEmailService>();
            return services;
        }

        public static IServiceCollection AddMailModule(this IServiceCollection services, System.Action<IServiceCollection> registerAdditional)
        {
            services.AddTransient<IEmailService, SmtpEmailService>();
            registerAdditional?.Invoke(services);
            return services;
        }
    }
}
