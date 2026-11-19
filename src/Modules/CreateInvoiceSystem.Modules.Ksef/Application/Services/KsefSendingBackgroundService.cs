using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Services;

public class KsefSendingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<KsefSendingBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IKsefOutboxRepository>();

                var pending = await outboxRepository.GetPendingAsync(stoppingToken);

                foreach (var item in pending)
                {
                    try
                    {
                        await ProcessItemAsync(scope.ServiceProvider, outboxRepository, item, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Nie udało się przygotować faktury {InvoiceId} do wysyłki KSeF.", item.InvoiceId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Błąd w pętli workera wysyłki KSeF.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessItemAsync(
        IServiceProvider scopedProvider,
        IKsefOutboxRepository outboxRepository,
        KsefOutboxItem item,
        CancellationToken cancellationToken)
    {
        var credentialsStore = scopedProvider.GetRequiredService<IKsefCredentialsStore>();
        if (!await credentialsStore.IsConfiguredAsync(item.UserId, cancellationToken))
        {
            logger.LogWarning("Faktura {InvoiceId} oczekuje na konfigurację tokenu KSeF użytkownika {UserId}.", item.InvoiceId, item.UserId);
            return;
        }

        await credentialsStore.GetRequiredAsync(item.UserId, cancellationToken);
        await outboxRepository.MarkInProgressAsync(item.Id, cancellationToken);
        await outboxRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var xmlGenerator = scopedProvider.GetRequiredService<IKsefInvoiceXmlGenerator>();
            var apiClient = scopedProvider.GetRequiredService<IKsefApiClient>();

            var xml = await xmlGenerator.GenerateXmlAsync(item.UserId, item.InvoiceId, cancellationToken);
            var result = await apiClient.SendInvoiceAsync(item.UserId, xml, cancellationToken);

            await outboxRepository.MarkSucceededAsync(item.Id, result, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Faktura {InvoiceId} wysłana do KSeF. Numer KSeF: {KsefNumber}.", item.InvoiceId, result.KsefNumber);
        }
        catch (KsefInvoiceRejectedException ex)
        {
            await outboxRepository.MarkRejectedAsync(item.Id, ex.Message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);
            logger.LogWarning(ex, "KSeF odrzucił fakturę {InvoiceId}.", item.InvoiceId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await outboxRepository.MarkFailedAsync(item.Id, ex.Message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);
            logger.LogError(ex, "Błąd wysyłki faktury {InvoiceId} do KSeF.", item.InvoiceId);
        }
    }
}