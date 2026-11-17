using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers
{
    public class UserDeletedInvoicesHandler(IInvoiceRepository _invoiceRepository)
    : INotificationHandler<UserDeletedNotification>
    {
        public async Task Handle(UserDeletedNotification notification, CancellationToken ct)
        {
            await _invoiceRepository.RemoveAllByUserIdAsync(notification.UserId, ct);
        }
    }
}