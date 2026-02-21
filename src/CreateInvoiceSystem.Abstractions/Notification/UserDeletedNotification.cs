using MediatR;

namespace CreateInvoiceSystem.Abstractions.Notification;

public record UserDeletedNotification(int UserId) : INotification;
