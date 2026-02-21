using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers
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
