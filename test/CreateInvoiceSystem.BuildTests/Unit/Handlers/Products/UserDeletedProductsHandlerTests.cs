using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Products.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Interfaces;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Products
{
    public class UserDeletedProductsHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly UserDeletedProductsHandler _handler;

        public UserDeletedProductsHandlerTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _handler = new UserDeletedProductsHandler(_productRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Call_RemoveAllByUserIdAsync_With_Correct_UserId()
        {
            var notification = new UserDeletedNotification(123);
            var cancellationToken = new CancellationToken();

            await _handler.Handle(notification, cancellationToken);

            _productRepositoryMock.Verify(
                x => x.RemoveAllByUserIdAsync(123, cancellationToken),
                Times.Once);
        }
    }
}
