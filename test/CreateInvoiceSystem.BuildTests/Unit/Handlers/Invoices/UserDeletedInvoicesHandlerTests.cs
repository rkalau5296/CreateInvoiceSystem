using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Invoices
{
    public class UserDeletedInvoicesHandlerTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly UserDeletedInvoicesHandler _handler;

        public UserDeletedInvoicesHandlerTests()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new UserDeletedInvoicesHandler(_invoiceRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Call_RemoveAllByUserIdAsync_With_Correct_UserId()
        {
            var notification = new UserDeletedNotification(123);
            var cancellationToken = new CancellationToken();

            await _handler.Handle(notification, cancellationToken);

            _invoiceRepositoryMock.Verify(
                x => x.RemoveAllByUserIdAsync(123, cancellationToken),
                Times.Once);
        }
    }
}
