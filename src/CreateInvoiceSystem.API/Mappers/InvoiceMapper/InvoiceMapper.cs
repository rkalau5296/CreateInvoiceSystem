using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Shared.Persistence;

namespace CreateInvoiceSystem.API.Mappers.InvoiceMapper;

public static class InvoiceMapper
{
    public static AddressEntity ToAddressEntity(Address a)
    {
        return new AddressEntity
        {
            AddressId = a?.AddressId ?? 0,
            Street = a?.Street ?? string.Empty, 
            Number = a?.Number ?? string.Empty,
            City = a?.City ?? string.Empty,
            PostalCode = a?.PostalCode ?? string.Empty,
            Country = a?.Country ?? string.Empty
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
        var resolvedClientId = clientId ?? invoice.ClientId;

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
            ClientId = resolvedClientId > 0
            ? resolvedClientId
            : null,
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

    public static InvoicePositionEntity ToInvoicePositionEntity(InvoicePosition ip)
    {
        return new InvoicePositionEntity
        {
            InvoicePositionId = ip.InvoicePositionId,
            ProductId = ip.ProductId > 0 ? ip.ProductId : null,
            ProductName = ip.ProductName,
            ProductDescription = ip.ProductDescription,
            ProductValue = ip.ProductValue,
            Quantity = ip.Quantity,
            VatRate = ip.VatRate
        };
    }

    public static List<InvoicePositionEntity> ToInvoicePositionEntities(IEnumerable<InvoicePosition> positions)
    {
        return positions.Select(p => ToInvoicePositionEntity(p)).ToList();
    }

    public static ProductEntity ToProductEntity(Product p)
    {
        return new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Value = p.Value,
            UserId = p.UserId
        };
    }

    public static Client MapClient(ClientEntity? clientEntity, AddressEntity? addressEntity)
    {
        if (clientEntity == null) return null!;

        return new Client
        {
            ClientId = clientEntity.ClientId,
            Name = clientEntity.Name,
            Nip = clientEntity.Nip,
            AddressId = clientEntity.AddressId,
            UserId = clientEntity.UserId,
            Email = clientEntity.Email,
            Address = addressEntity == null ? null! : new Address
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
        if (p == null) return null!;

        return new Product
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Value = p.Value,
            UserId = p.UserId
        };
    }

    public static InvoicePosition MapPosition(InvoicePositionEntity e, IDictionary<int, ProductEntity>? productsMap = null)
    {
        Product prod = null!;
        if (e.ProductId.HasValue && productsMap != null && productsMap.TryGetValue(e.ProductId.Value, out var p))
            prod = MapProduct(p);

        return new InvoicePosition
        {
            InvoicePositionId = e.InvoicePositionId,
            InvoiceId = e.InvoiceId,
            ProductId = e.ProductId,
            ProductName = e.ProductName,
            ProductDescription = e.ProductDescription ?? string.Empty,
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
            Comments = e.Comments ?? string.Empty,
            ClientId = e.ClientId,
            UserId = e.UserId,
            MethodOfPayment = e.MethodOfPayment,
            SellerName = e.SellerName ?? string.Empty,
            SellerNip = e.SellerNip ?? string.Empty,
            SellerAddress = e.SellerAddress ?? string.Empty,
            BankAccountNumber = e.BankAccountNumber ?? string.Empty,
            ClientName = e.ClientName ?? string.Empty,
            ClientEmail = e.ClientEmail ?? string.Empty,
            ClientAddress = e.ClientAddress ?? string.Empty,
            ClientNip = e.ClientNip ?? string.Empty,
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
            Comments = e.Comments ?? string.Empty,
            ClientId = e.ClientId,
            UserId = e.UserId,
            MethodOfPayment = e.MethodOfPayment,
            SellerName = e.SellerName ?? string.Empty,
            SellerNip = e.SellerNip ?? string.Empty,
            SellerAddress = e.SellerAddress ?? string.Empty,
            BankAccountNumber = e.BankAccountNumber ?? string.Empty,
            ClientName = e.ClientName ?? string.Empty,
            ClientAddress = e.ClientAddress ?? string.Empty,
            ClientNip = e.ClientNip ?? string.Empty,
            ClientEmail = e.ClientEmail ?? string.Empty,        
            Client = clientEntity == null ? null! : new Client
            {
                ClientId = clientEntity.ClientId,
                Name = clientEntity.Name,
                Nip = clientEntity.Nip
            },
            InvoicePositions = [..positions.Select(ip => new InvoicePosition
            {
                InvoicePositionId = ip.InvoicePositionId,
                InvoiceId = ip.InvoiceId,
                ProductId = ip.ProductId,
                ProductName = ip.ProductName,
                ProductDescription = ip.ProductDescription ?? string.Empty,
                ProductValue = ip.ProductValue,
                Quantity = ip.Quantity,
                VatRate = ip.VatRate,
                Product = null!
            })]
        };
    }
    public static User Map(UserEntity? u, AddressEntity? a)
    {
        if (u == null) return null!;

        return new User     
        {
            UserId = u.Id,
            Name = u.Name,
            CompanyName = u.CompanyName,
            Nip = u.Nip,
            AddressId = u.AddressId,
            BankAccountNumber = u.BankAccountNumber ?? string.Empty,
            Address = a == null ? null! : new Address
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