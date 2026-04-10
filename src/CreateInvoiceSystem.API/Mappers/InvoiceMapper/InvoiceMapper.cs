using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.API.Mappers.InvoiceMapper;

public static class InvoiceMapper
{
    public static AddressEntity ToAddressEntity(Address a)
    {
        return new AddressEntity
        {
            AddressId = a?.AddressId ?? 0,
            Street = a?.Street,
            Number = a?.Number,
            City = a?.City,
            PostalCode = a?.PostalCode,
            Country = a?.Country
        };
    }

    public static ClientEntity ToClientEntity(Client c)
    {
        return new ClientEntity
        {
            ClientId = c?.ClientId ?? 0,
            Name = c?.Name,
            Nip = c?.Nip,
            AddressId = c?.AddressId ?? 0,
            UserId = c?.UserId ?? 0,
            Email = c?.Email
        };
    }

    public static InvoiceEntity ToInvoiceEntity(Invoice invoice, int? clientId = null)
    {
        return new InvoiceEntity
        {
            InvoiceId = invoice.InvoiceId,
            Title = invoice.Title,
            TotalNet = invoice.TotalNet,
            TotalVat = invoice.TotalVat,
            TotalGross = invoice.TotalGross,
            PaymentDate = invoice.PaymentDate,
            CreatedDate = invoice.CreatedDate,
            Comments = invoice.Comments,
            ClientId = clientId ?? invoice.ClientId,
            UserId = invoice.UserId,
            MethodOfPayment = invoice.MethodOfPayment,
            SellerName = invoice.SellerName,
            SellerNip = invoice.SellerNip,
            SellerAddress = invoice.SellerAddress,
            BankAccountNumber = invoice.BankAccountNumber,
            ClientName = invoice.ClientName,
            ClientAddress = invoice.ClientAddress,
            ClientNip = invoice.ClientNip,            
            ClientEmail = invoice.ClientEmail
        };
    }

    public static InvoicePositionEntity ToInvoicePositionEntity(InvoicePosition ip, int invoiceId)
    {
        return new InvoicePositionEntity
        {
            InvoicePositionId = ip.InvoicePositionId,
            InvoiceId = invoiceId,
            ProductId = ip.ProductId,
            ProductName = ip.ProductName,
            ProductDescription = ip.ProductDescription,
            ProductValue = ip.ProductValue,
            Quantity = ip.Quantity,
            VatRate = ip.VatRate
        };
    }

    public static List<InvoicePositionEntity> ToInvoicePositionEntities(IEnumerable<InvoicePosition> positions, int invoiceId)
    {
        return positions.Select(p => ToInvoicePositionEntity(p, invoiceId)).ToList();
    }

    public static ProductEntity ToProductEntity(Product p)
    {
        return new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Value = p.Value,
            UserId = p.UserId,
            IsDeleted = p.IsDeleted
        };
    }

    public static Client MapClient(ClientEntity? clientEntity, AddressEntity? addressEntity)
    {
        if (clientEntity == null) return null;

        return new Client
        {
            ClientId = clientEntity.ClientId,
            Name = clientEntity.Name,
            Nip = clientEntity.Nip,
            AddressId = clientEntity.AddressId,
            UserId = clientEntity.UserId,
            Email = clientEntity.Email,
            Address = addressEntity == null ? null : new Address
            {
                AddressId = addressEntity.AddressId,
                Street = addressEntity.Street,
                Number = addressEntity.Number,
                City = addressEntity.City,
                PostalCode = addressEntity.PostalCode,
                Country = addressEntity.Country
            }
        };
    }

    public static Product MapProduct(ProductEntity p)
    {
        if (p == null) return null;

        return new Product
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Value = p.Value,
            UserId = p.UserId,
            IsDeleted = p.IsDeleted
        };
    }

    public static InvoicePosition MapPosition(InvoicePositionEntity e, IDictionary<int, ProductEntity>? productsMap = null)
    {
        Product prod = null;
        if (e.ProductId.HasValue && productsMap != null && productsMap.TryGetValue(e.ProductId.Value, out var p))
            prod = MapProduct(p);

        return new InvoicePosition
        {
            InvoicePositionId = e.InvoicePositionId,
            InvoiceId = e.InvoiceId,
            ProductId = e.ProductId,
            ProductName = e.ProductName,
            ProductDescription = e.ProductDescription,
            ProductValue = e.ProductValue,
            Quantity = e.Quantity,
            VatRate = e.VatRate,
            Product = prod
        };
    }

    public static List<InvoicePosition> MapPositions(IEnumerable<InvoicePositionEntity> entities, IDictionary<int, ProductEntity>? productsMap = null)
    {
        return entities.Select(e => MapPosition(e, productsMap)).ToList();
    }

    public static Invoice MapDetailed(
        InvoiceEntity e,
        ClientEntity? clientEntity,
        AddressEntity? addressEntity,
        IEnumerable<InvoicePositionEntity> positions,
        IDictionary<int, ProductEntity>? productsMap = null)
    {
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
            SellerName = e.SellerName,
            SellerNip = e.SellerNip,
            SellerAddress = e.SellerAddress,
            BankAccountNumber = e.BankAccountNumber,
            ClientName = e.ClientName,
            ClientEmail = e.ClientEmail,
            ClientAddress = e.ClientAddress,
            ClientNip = e.ClientNip,
            Client = MapClient(clientEntity, addressEntity),
            InvoicePositions = MapPositions(positions, productsMap)
        };
    }

    public static Invoice MapSummary(
        InvoiceEntity e,
        IEnumerable<InvoicePositionEntity> positions,
        ClientEntity? clientEntity = null)
    {
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
            SellerName = e.SellerName,
            SellerNip = e.SellerNip,
            SellerAddress = e.SellerAddress,
            BankAccountNumber = e.BankAccountNumber,
            ClientName = e.ClientName,
            ClientAddress = e.ClientAddress,
            ClientNip = e.ClientNip,
            Client = clientEntity == null ? null : new Client
            {
                ClientId = clientEntity.ClientId,
                Name = clientEntity.Name,
                Nip = clientEntity.Nip
            },
            InvoicePositions = positions.Select(ip => new InvoicePosition
            {
                InvoicePositionId = ip.InvoicePositionId,
                InvoiceId = ip.InvoiceId,
                ProductId = ip.ProductId,
                ProductName = ip.ProductName,
                ProductDescription = ip.ProductDescription,
                ProductValue = ip.ProductValue,
                Quantity = ip.Quantity,
                VatRate = ip.VatRate,
                Product = null
            }).ToList()
        };
    }
    public static User? Map(UserEntity? u, AddressEntity? a)
    {
        if (u == null) return null;

        return new User     
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber,
            Address = a == null ? null : new Address
            {
                AddressId = a.AddressId,
                Street = a.Street,
                Number = a.Number,
                City = a.City,
                PostalCode = a.PostalCode,
                Country = a.Country
            }
        };
    }
}