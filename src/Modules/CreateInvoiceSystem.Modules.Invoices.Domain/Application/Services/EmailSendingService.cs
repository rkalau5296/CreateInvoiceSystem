using CreateInvoiceSystem.Modules.Invoices.Domain.BackgroundTasks;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Services
{
    public class EmailSendingService : BackgroundService
    {        
        private readonly ILogger<EmailSendingService> _logger;        
        private readonly ChannelReader<EmailTask> _reader;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailSendingService(ILogger<EmailSendingService> logger, ChannelReader<EmailTask> reader, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _reader = reader;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {            
            await foreach (EmailTask task in _reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IInvoiceEmailSender>();
                                        
                    switch (task)
                    {
                        case SellerEmailTask sellerTask:
                            
                            await emailSender.SendInvoiceCreatedEmailAsync(
                                sellerTask.UserEmail,
                                sellerTask.InvoiceTitle,
                                cancellationToken);
                            break;

                        case ClientEmailTask clientTask:
                            
                            await emailSender.SendInvoiceToClientCreatedAsync(                                
                                clientTask.Invoice,
                                cancellationToken);
                            break;

                        default:
                            _logger.LogWarning("Nieobsługiwany typ zadania e-mail: {TaskType}", task.GetType().Name);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wysyłania wiadomości e-mail.");
                }
            }
        }
    }
}
