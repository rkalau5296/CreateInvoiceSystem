using CreateInvoiceSystem.Modules.Invoices.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Invoices;

public class CloneInvoiceHandlerTests
{
    private const int UserId = 7;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private readonly Mock<IInvoiceRepository> _repositoryMock = new();
    private Invoice? _saved;

    private CloneInvoiceHandler CreateSut(DateTimeOffset? utcNow = null)
    {
        _repositoryMock
            .Setup(r => r.GetUserByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                UserId = UserId,
                CompanyName = "Nowa Nazwa Firmy",
                Nip = "1234567890",
                BankAccountNumber = "PL00000000000000000000000000",
                Address = new Address { Street = "Sprzedawcy", Number = "1", City = "Warszawa", PostalCode = "00-001", Country = "Polska" }
            });

        _repositoryMock
            .Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Callback((Invoice inv, CancellationToken _) => _saved = inv)
            .ReturnsAsync((Invoice inv, CancellationToken _) => inv);

        return new CloneInvoiceHandler(_repositoryMock.Object,
            new FixedTimeProvider(utcNow ?? new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero)));
    }

    private void SetupSource(Invoice source) =>
        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(UserId, source.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

    private static Invoice CreateSource() => new()
    {
        InvoiceId = 5,
        Title = "3/10/2026",
        UserId = UserId,
        CreatedDate = new DateTime(2026, 10, 1),
        PaymentDate = new DateTime(2026, 10, 31),
        Comments = "Uwagi",
        MethodOfPayment = "Przelew",
        Currency = "EUR",
        ClientName = "Klient",
        ClientNip = "9876543210",
        ClientAddress = "Klienta 10, Kraków, 30-001, Polska",
        ClientEmail = "klient@test.pl",
        IsKsefLocked = true,
        InvoicePositions =
        [
            new InvoicePosition { InvoicePositionId = 11, InvoiceId = 5, ProductName = "Usługa", ProductDescription = "Opis", ProductValue = 123.45m, Quantity = 2, UnitOfMeasure = "godz.", VatRate = "8%" },
            new InvoicePosition { InvoicePositionId = 12, InvoiceId = 5, ProductName = "Towar", ProductDescription = "", ProductValue = 10m, Quantity = 1, UnitOfMeasure = "szt.", VatRate = "zw" }
        ]
    };

    [Fact]
    public async Task Handle_ShouldUseNextNumberAndTodayDates()
    {
        var source = CreateSource();
        SetupSource(source);
        var sut = CreateSut();
        _repositoryMock
            .Setup(r => r.GetMaxInvoiceNumberInMonthAsync(UserId, 11, 2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        var result = await sut.Handle(new CloneInvoiceRequest(5) { UserId = UserId }, CancellationToken.None);

        result.Data.Title.Should().Be("5/11/2026");
        result.Data.CreatedDate.Should().Be(new DateTime(2026, 11, 19));
        result.Data.PaymentDate.Should().Be(new DateTime(2026, 12, 19)); // ten sam termin: 30 dni
    }

    [Fact]
    public async Task Handle_ShouldCopyPositionsClientCurrencyAndPaymentData()
    {
        var source = CreateSource();
        SetupSource(source);
        var sut = CreateSut();

        await sut.Handle(new CloneInvoiceRequest(5) { UserId = UserId }, CancellationToken.None);

        _saved.Should().NotBeNull();
        _saved!.InvoiceId.Should().Be(0);
        _saved.Currency.Should().Be("EUR");
        _saved.MethodOfPayment.Should().Be("Przelew");
        _saved.Comments.Should().Be("Uwagi");
        _saved.ClientName.Should().Be("Klient");
        _saved.ClientNip.Should().Be("9876543210");
        _saved.ClientAddress.Should().Be(source.ClientAddress);
        _saved.ClientEmail.Should().Be("klient@test.pl");
        _saved.IsKsefLocked.Should().BeFalse();
        _saved.IsKsefAccepted.Should().BeNull();

        _saved.InvoicePositions.Should().HaveCount(2);
        _saved.InvoicePositions.Should().OnlyContain(p => p.InvoicePositionId == 0 && p.InvoiceId == 0);
        var first = _saved.InvoicePositions.First();
        first.ProductName.Should().Be("Usługa");
        first.ProductValue.Should().Be(123.45m);
        first.Quantity.Should().Be(2);
        first.UnitOfMeasure.Should().Be("godz.");
        first.VatRate.Should().Be("8%");

        _saved.TotalNet.Should().Be(256.90m);
        _saved.TotalVat.Should().Be(19.75m);
        _saved.TotalGross.Should().Be(276.65m);
    }

    [Fact]
    public async Task Handle_ShouldTakeSellerDataFromCurrentUserProfile()
    {
        var source = CreateSource();
        source.SellerName = "Stara Nazwa";
        SetupSource(source);
        var sut = CreateSut();

        await sut.Handle(new CloneInvoiceRequest(5) { UserId = UserId }, CancellationToken.None);

        _saved!.SellerName.Should().Be("Nowa Nazwa Firmy");
        _saved.SellerNip.Should().Be("1234567890");
        _saved.BankAccountNumber.Should().Be("PL00000000000000000000000000");
        _saved.SellerAddress.Should().Contain("Warszawa");
    }

    [Fact]
    public async Task Handle_ShouldUseWarsawDate_WhenUtcIsStillPreviousDay()
    {
        var source = CreateSource();
        SetupSource(source);
        var sut = CreateSut(new DateTimeOffset(2026, 11, 30, 23, 30, 0, TimeSpan.Zero)); // 00:30 1 grudnia w Warszawie
        _repositoryMock
            .Setup(r => r.GetMaxInvoiceNumberInMonthAsync(UserId, 12, 2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await sut.Handle(new CloneInvoiceRequest(5) { UserId = UserId }, CancellationToken.None);

        result.Data.Title.Should().Be("1/12/2026");
        result.Data.CreatedDate.Should().Be(new DateTime(2026, 12, 1));
    }

    [Fact]
    public async Task Handle_ShouldFallBackTo14Days_WhenSourcePaymentDateIsBeforeCreatedDate()
    {
        var source = CreateSource();
        source.PaymentDate = source.CreatedDate.AddDays(-3);
        SetupSource(source);
        var sut = CreateSut();

        var result = await sut.Handle(new CloneInvoiceRequest(5) { UserId = UserId }, CancellationToken.None);

        result.Data.PaymentDate.Should().Be(new DateTime(2026, 12, 3));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInvoiceNotFound()
    {
        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(UserId, 99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invoice with ID 99 not found."));
        var sut = CreateSut();

        var act = () => sut.Handle(new CloneInvoiceRequest(99) { UserId = UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repositoryMock.Verify(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
