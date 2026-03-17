using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;


namespace CreateInvoiceSystem.API.Mappers.UserMapper;

internal static class UserMapper
{
    public static AddressEntity ToAddressEntity(Address a)
    {
        if (a == null) return null;
        return new AddressEntity
        {
            AddressId = a.AddressId,
            Street = a.Street,
            Number = a.Number,
            City = a.City,
            PostalCode = a.PostalCode,
            Country = a.Country
        };
    }

    public static UserEntity ToUserEntity(User u)
    {
        if (u == null) return null;
        return new UserEntity
        {
            Id = u.UserId,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Email = u.Email,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public static Address MapAddress(AddressEntity e)
    {
        if (e == null) return null;
        return new Address
        {
            AddressId = e.AddressId,
            Street = e.Street,
            Number = e.Number,
            City = e.City,
            PostalCode = e.PostalCode,
            Country = e.Country
        };
    }

    public static Product MapProduct(ProductEntity e)
    {
        if (e == null) return null;
        return new Product
        {
            ProductId = e.ProductId,
            Name = e.Name,
            Description = e.Description,
            Value = e.Value,
            UserId = e.UserId,
            IsDeleted = e.IsDeleted
        };
    }

    public static InvoicePosition MapInvoicePosition(InvoicePositionEntity e)
    {
        if (e == null) return null;
        return new InvoicePosition
        {
            InvoicePositionId = e.InvoicePositionId,
            InvoiceId = e.InvoiceId,
            ProductId = e.ProductId,
            Quantity = e.Quantity,
            ProductDescription = e.ProductDescription,
            ProductName = e.ProductName,
            ProductValue = e.ProductValue,
            VatRate = e.VatRate
        };
    }

    public static Invoice MapInvoice(InvoiceEntity e, IEnumerable<InvoicePositionEntity> allPositions)
    {
        if (e == null) return null;
        var positions = allPositions?.Where(ip => ip.InvoiceId == e.InvoiceId).Select(MapInvoicePosition).ToList() ?? new List<InvoicePosition>();
        return new Invoice
        {
            InvoiceId = e.InvoiceId,
            Title = e.Title,
            TotalNet = e.TotalNet,
            TotalVat = e.TotalVat,
            TotalGross = e.TotalGross,
            PaymentDate = e.PaymentDate,
            CreatedDate = e.CreatedDate,
            Comments = e.Comments,
            ClientId = e.ClientId,
            UserId = e.UserId,
            MethodOfPayment = e.MethodOfPayment,
            ClientName = e.ClientName,
            ClientAddress = e.ClientAddress,
            ClientNip = e.ClientNip,
            InvoicePositions = positions
        };
    }

    public static Client MapClient(ClientEntity c, AddressEntity addr)
    {
        if (c == null) return null;
        return new Client
        {
            ClientId = c.ClientId,
            Name = c.Name,
            Nip = c.Nip,
            UserId = c.UserId,
            Address = addr == null ? null : MapAddress(addr)
        };
    }

    public static User MapFull(
        UserEntity u,
        AddressEntity addr,
        IEnumerable<InvoiceEntity> invoices,
        IEnumerable<InvoicePositionEntity> invoicePositions,
        IEnumerable<ProductEntity> products,
        IEnumerable<ClientEntity> clients,
        IEnumerable<AddressEntity> clientAddresses)
    {
        if (u == null) return null;

        var mappedInvoices = invoices?.Select(i => MapInvoice(i, invoicePositions)).ToList() ?? new List<Invoice>();

        var mappedProducts = products?.Select(MapProduct).ToList() ?? new List<Product>();

        var mappedClients = clients?.Select(c =>
        {
            var clientAddr = clientAddresses?.FirstOrDefault(a => a.AddressId == c.AddressId);
            return new Client
            {
                ClientId = c.ClientId,
                Name = c.Name,
                Nip = c.Nip,
                UserId = c.UserId,
                Address = clientAddr == null ? null : MapAddress(clientAddr)
            };
        }).ToList() ?? new List<Client>();

        return new User
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Email = u.Email,
            Nip = u.Nip,
            BankAccountNumber = u.BankAccountNumber,
            AddressId = u.AddressId,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Address = addr == null ? null : MapAddress(addr),
            Invoices = mappedInvoices,
            Products = mappedProducts,
            Clients = mappedClients
        };
    }

    public static User MapSummary(UserEntity u, AddressEntity addr, IEnumerable<InvoiceEntity> allInvoices, IEnumerable<InvoicePositionEntity> allInvoicePositions, IEnumerable<ProductEntity> allProducts, IEnumerable<ClientEntity> allClients, IEnumerable<AddressEntity> allClientAddresses)
    {
        if (u == null) return null;

        var invoices = allInvoices?.Where(i => i.UserId == u.Id).Select(i => MapInvoice(i, allInvoicePositions)).ToList() ?? new List<Invoice>();

        var products = allProducts?.Where(p => p.UserId == u.Id).Select(MapProduct).ToList() ?? new List<Product>();

        var clients = allClients?.Where(c => c.UserId == u.Id).Select(c =>
        {
            var clientAddr = allClientAddresses?.FirstOrDefault(a => a.AddressId == c.AddressId);
            return new Client
            {
                ClientId = c.ClientId,
                Name = c.Name,
                Nip = c.Nip,
                UserId = c.UserId,
                Address = clientAddr == null ? null : MapAddress(clientAddr)
            };
        }).ToList() ?? new List<Client>();

        return new User
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Email = u.Email,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber,
            IsActive = u.IsActive,
            Address = addr == null ? null : MapAddress(addr),
            Invoices = invoices,
            Products = products,
            Clients = clients
        };
    }

    public static User MapLight(UserEntity u, AddressEntity addr)
    {
        if (u == null) return null;
        return new User
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Email = u.Email,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber,
            IsActive = u.IsActive,
            Address = addr == null ? null : MapAddress(addr),
            CreatedAt = u.CreatedAt
        };
    }

    public static User MapLight(UserEntity u)
    {
        if (u == null) return null;
        return new User
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Email = u.Email,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public static UserSessionEntity ToUserSessionEntity(UserSession s)
    {
        if (s == null) return null;
        return new UserSessionEntity
        {
            Id = s.Id,
            UserId = s.UserId,
            RefreshToken = s.RefreshToken,
            LastActivityAt = s.LastActivityAt,
            IsRevoked = s.IsRevoked
        };
    }

    public static UserSession ToUserSession(UserSessionEntity e)
    {
        if (e == null) return null;
        return new UserSession
        {
            Id = e.Id,
            UserId = e.UserId,
            RefreshToken = e.RefreshToken,
            LastActivityAt = e.LastActivityAt,
            IsRevoked = e.IsRevoked
        };
    }
}