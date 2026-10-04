using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Clients.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Application.Handlers;

public class UserDeletedClientsHandler(IClientRepository _clientRepository)
 : INotificationHandler<UserDeletedNotification>
{
    public async Task Handle(UserDeletedNotification notification, CancellationToken ct)
    {
        await _clientRepository.RemoveAllByUserIdAsync(notification.UserId, ct);
    }
}