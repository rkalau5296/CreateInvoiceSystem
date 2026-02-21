using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;

public class UserDeletedProductsHandler(IProductRepository _productRepository)
: INotificationHandler<UserDeletedNotification>
{
    public async Task Handle(UserDeletedNotification notification, CancellationToken ct)
    {
        await _productRepository.RemoveAllByUserIdAsync(notification.UserId, ct);
    }
}
