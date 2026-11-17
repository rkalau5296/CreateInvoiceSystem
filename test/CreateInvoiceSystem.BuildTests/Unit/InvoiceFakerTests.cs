extern alias seed;

using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using seed::CreateInvoiceSystem.Persistence.Seed.Mock;
using CreateInvoiceSystem.Shared.Persistence;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class InvoiceFakerTests
{
    [Fact]
    public void Existing_client_without_nip_is_rejected()
    {
        var client = new ClientEntity { Name = "Klient", Nip = null };

        var error = Assert.Throws<InvalidOperationException>(() =>
            InvoiceFaker.Generate(new UserEntity(), InvoiceScenario.ExistingClient_NewProducts,
                client, new AddressEntity()));

        Assert.Equal("Kontrahent nie ma numeru NIP.", error.Message);
    }

    [Fact]
    public void Existing_client_without_address_is_rejected()
    {
        var client = new ClientEntity { Name = "Klient", Nip = "1234567890" };

        var error = Assert.Throws<ArgumentNullException>(() =>
            InvoiceFaker.Generate(new UserEntity(), InvoiceScenario.ExistingClient_NewProducts, client));

        Assert.Equal("dbClientAddress", error.ParamName);
    }

    [Fact]
    public void Existing_client_details_are_copied()
    {
        var client = new ClientEntity { Name = "Klient", Nip = "1234567890" };
        var address = new AddressEntity { Street = "Ulica", Number = "1", City = "Miasto", PostalCode = "00-000", Country = "Polska" };

        var invoice = InvoiceFaker.Generate(new UserEntity(), InvoiceScenario.ExistingClient_NewProducts, client, address);

        Assert.Equal(client.Nip, invoice.ClientNip);
        Assert.Equal("Ulica 1, Miasto, 00-000, Polska", invoice.ClientAddress);
    }
}

