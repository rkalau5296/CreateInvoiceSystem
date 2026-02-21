using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;

public class UserDeletedClientsHandler(IClientRepository _clientRepository)
 : INotificationHandler<UserDeletedNotification>
{
    public async Task Handle(UserDeletedNotification notification, CancellationToken ct)
    {
        await _clientRepository.RemoveAllByUserIdAsync(notification.UserId, ct);
    }
}
