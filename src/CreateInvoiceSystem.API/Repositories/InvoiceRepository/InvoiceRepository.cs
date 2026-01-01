using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.InvoiceRepository
{
    public class InvoiceRepository(IDbContext db) : IInvoiceRepository
    {
        private readonly IDbContext _db = db;

        public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
        {
            var addressEntity = new AddressEntity
            {
                Street = client.Address.Street,
                Number = client.Address.Number,
                City = client.Address.City,
                PostalCode = client.Address.PostalCode,
                Country = client.Address.Country
            };

            await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var clientEntity = new ClientEntity
            {
                Name = client.Name,
                Nip = client.Nip,
                UserId = client.UserId,
                AddressId = addressEntity.AddressId
            };
            await _db.Set<ClientEntity>().AddAsync(clientEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            client.ClientId = clientEntity.ClientId;
            client.AddressId = addressEntity.AddressId;
        }

        public async Task<Invoice> AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
        {            
            var invoiceEntity = new InvoiceEntity
            {
                Title = invoice.Title,
                TotalAmount = invoice.TotalAmount,
                PaymentDate = invoice.PaymentDate,
                CreatedDate = invoice.CreatedDate,
                Comments = invoice.Comments,
                ClientId = invoice.ClientId,
                UserId = invoice.UserId,
                MethodOfPayment = invoice.MethodOfPayment,
                ClientName = invoice.ClientName,
                ClientAddress = invoice.ClientAddress,
                ClientNip = invoice.ClientNip,
                
                InvoicePositions = invoice.InvoicePositions.Select(ip => new InvoicePositionEntity
                {
                    ProductId = ip.ProductId,
                    ProductName = ip.ProductName,
                    ProductDescription = ip.ProductDescription,
                    ProductValue = ip.ProductValue,
                    Quantity = ip.Quantity
                }).ToList()
            };
            
            await _db.Set<InvoiceEntity>().AddAsync(invoiceEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            
            var positionsToSave = invoiceEntity.InvoicePositions
                .Cast<InvoicePositionEntity>()
                .Select(pe =>
                {
                    pe.InvoiceId = invoiceEntity.InvoiceId;
                    return pe;
                }).ToList();
            
            await _db.Set<InvoicePositionEntity>().AddRangeAsync(positionsToSave, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Invoice
            {
                InvoiceId = invoiceEntity.InvoiceId,
                Title = invoiceEntity.Title,
                TotalAmount = invoiceEntity.TotalAmount,
                PaymentDate = invoiceEntity.PaymentDate,
                CreatedDate = invoiceEntity.CreatedDate,
                Comments = invoiceEntity.Comments,
                ClientId = invoiceEntity.ClientId,
                UserId = invoiceEntity.UserId,
                MethodOfPayment = invoiceEntity.MethodOfPayment,
                ClientName = invoiceEntity.ClientName,
                ClientAddress = invoiceEntity.ClientAddress,
                ClientNip = invoiceEntity.ClientNip,                
                InvoicePositions = [.. positionsToSave.Select(pe => new InvoicePosition
                {
                    InvoicePositionId = pe.InvoicePositionId,
                    InvoiceId = pe.InvoiceId,
                    ProductId = pe.ProductId,
                    ProductName = pe.ProductName,
                    ProductDescription = pe.ProductDescription,
                    ProductValue = pe.ProductValue,
                    Quantity = pe.Quantity
                })]
            };
        }

        public Task AddInvoicePositionAsync(ICollection<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
        {
            return _db.Set<InvoicePositionEntity>().AddRangeAsync(invoicePositions.Select(ip => new InvoicePositionEntity
            {
                ProductId = ip.ProductId,
                ProductName = ip.ProductName,
                ProductDescription = ip.ProductDescription,
                ProductValue = ip.ProductValue,
                Quantity = ip.Quantity
            }), cancellationToken);
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
            var entity = new ProductEntity
            {
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                UserId = product.UserId,
                IsDeleted = product.IsDeleted
            };

            await _db.Set<ProductEntity>().AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            product.ProductId = entity.ProductId;
        }

        public async Task<Client> GetClientAsync(string name, string street, string number, string city, string postalCode, string country, int userId, CancellationToken cancellationToken)
        {
            //var addressEntity = await _db.Set<AddressEntity>()
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(a =>
            //        a.Street == street &&
            //        a.Number == number &&
            //        a.City == city &&
            //        a.PostalCode == postalCode &&
            //        a.Country == country,
            //        cancellationToken);


            //var clientEntity = await _db.Set<ClientEntity>()
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(c =>
            //        c.Name == name &&
            //        c.UserId == userId &&
            //        c.AddressId == addressEntity.AddressId,
            //        cancellationToken);
            var clientEntity = await (from client in _db.Set<ClientEntity>().AsNoTracking()
                                join address in _db.Set<AddressEntity>().AsNoTracking()
                                on client.AddressId equals address.AddressId
                                where client.Name == name &&
                                      client.UserId == userId &&
                                      address.Street == street &&
                                      address.Number == number &&
                                      address.City == city &&
                                      address.PostalCode == postalCode &&
                                      address.Country == country
                                select new { client, address })
                        .FirstOrDefaultAsync(cancellationToken);

            if (clientEntity == null) return null;

            return new Client
            {
                ClientId = clientEntity.client.ClientId,
                Name = clientEntity.client.Name,
                Nip = clientEntity.client.Nip,
                UserId = clientEntity.client.UserId,
                AddressId = clientEntity.address.AddressId,
                Address = new Address
                {
                    AddressId = clientEntity.address.AddressId,
                    Street = clientEntity.address.Street,
                    Number = clientEntity.address.Number,
                    City = clientEntity.address.City,
                    PostalCode = clientEntity.address.PostalCode,
                    Country = clientEntity.address.Country
                }
            };
        }

        public async Task<Client> GetClientByIdAsync(int clientId, CancellationToken cancellationToken)
        {
            var client = await _db.Set<ClientEntity>()
                .AsNoTracking().SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
                ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");

            var address = await _db.Set<AddressEntity>()
                .AsNoTracking().SingleOrDefaultAsync(a => a.AddressId == client.AddressId, cancellationToken)
                ?? throw new InvalidOperationException($"Address with ID {client.AddressId} not found.");

            return new Client
            {
                ClientId = client.ClientId,
                Name = client.Name,
                Nip = client.Nip,
                AddressId = client.AddressId,
                UserId = client.UserId,
                Address = new Address
                {
                    AddressId = address.AddressId,
                    Street = address.Street,
                    Number = address.Number,
                    City = address.City,
                    PostalCode = address.PostalCode,
                    Country = address.Country
                },
            };
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId, CancellationToken cancellationToken)
        {
            var invoiceEntity = await _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.InvoiceId == invoiceId, cancellationToken)
                ?? throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");

            var clientEntity = await _db.Set<ClientEntity>().AsNoTracking()
                .SingleOrDefaultAsync(c => c.ClientId == invoiceEntity.ClientId, cancellationToken) ?? throw new InvalidOperationException($"Client with ID {invoiceEntity.ClientId} not found.");

            var addressEntity = clientEntity != null
                ? await _db.Set<AddressEntity>().AsNoTracking().SingleOrDefaultAsync(a => a.AddressId == clientEntity.AddressId, cancellationToken)
                : null;

            var invoicePositionsEntity = await _db.Set<InvoicePositionEntity>()
                .AsNoTracking()
                .Where(p => p.InvoiceId == invoiceId)
                .ToListAsync(cancellationToken);

            var productIds = invoicePositionsEntity
                .Select(p => p.ProductId)
                .Where(id => id.HasValue)
                .Distinct()
                .ToList();

            var productsMap = await _db.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, cancellationToken);

            return new Invoice
            {
                InvoiceId = invoiceEntity.InvoiceId,
                Title = invoiceEntity.Title,
                TotalAmount = invoiceEntity.TotalAmount,
                PaymentDate = invoiceEntity.PaymentDate,
                CreatedDate = invoiceEntity.CreatedDate,
                Comments = invoiceEntity.Comments,
                ClientId = invoiceEntity.ClientId,
                UserId = invoiceEntity.UserId,
                MethodOfPayment = invoiceEntity.MethodOfPayment,
                ClientName = invoiceEntity.ClientName,
                ClientAddress = invoiceEntity.ClientAddress,
                ClientNip = invoiceEntity.ClientNip,

                Client = clientEntity == null ? null : new Client
                {
                    ClientId = clientEntity.ClientId,
                    Name = clientEntity.Name,
                    Nip = clientEntity.Nip,
                    UserId = clientEntity.UserId,
                    Address = addressEntity == null ? null : new Address
                    {
                        AddressId = addressEntity.AddressId,
                        Street = addressEntity.Street,
                        Number = addressEntity.Number,
                        City = addressEntity.City,
                        PostalCode = addressEntity.PostalCode,
                        Country = addressEntity.Country
                    }
                },

                InvoicePositions = [.. invoicePositionsEntity.Select(ip => new InvoicePosition
                {
                    InvoicePositionId = ip.InvoicePositionId,
                    InvoiceId = ip.InvoiceId,
                    ProductId = ip.ProductId,
                    Quantity = ip.Quantity,
                    ProductName = ip.ProductName,
                    ProductDescription = ip.ProductDescription,
                    ProductValue = ip.ProductValue,
                    Product = (ip.ProductId.HasValue && productsMap.TryGetValue(ip.ProductId.Value, out var prod))
                        ? new Product
                        {
                            ProductId = prod.ProductId,
                            Name = prod.Name,
                            Description = prod.Description,
                            Value = prod.Value,
                            UserId = prod.UserId,
                            IsDeleted = prod.IsDeleted
                        }
                        : null
                })]
            };
        }

        public async Task<List<Invoice>> GetInvoicesAsync(CancellationToken cancellationToken)
        {
            var invoiceEntities = await _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (invoiceEntities.Count == 0 || invoiceEntities is null) return [];

            var invoiceIds = invoiceEntities.Select(i => i.InvoiceId).ToList();
            var clientIds = invoiceEntities.Select(i => i.ClientId).Where(id => id.HasValue).Distinct().ToList();

            var allPositions = await _db.Set<InvoicePositionEntity>()
                .AsNoTracking()
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .ToListAsync(cancellationToken);

            var allClients = await _db.Set<ClientEntity>()
                .AsNoTracking()
                .Where(c => clientIds.Contains(c.ClientId))
                .ToListAsync(cancellationToken);

            var positionsLookup = allPositions.ToLookup(p => p.InvoiceId);
            var clientsDict = allClients.ToDictionary(c => c.ClientId);

            return [.. invoiceEntities.Select(i => new Invoice
            {
                InvoiceId = i.InvoiceId,
                Title = i.Title,
                TotalAmount = i.TotalAmount,
                PaymentDate = i.PaymentDate,
                CreatedDate = i.CreatedDate,
                Comments = i.Comments,
                ClientId = i.ClientId,
                UserId = i.UserId,
                MethodOfPayment = i.MethodOfPayment,
                ClientName = i.ClientName,
                ClientAddress = i.ClientAddress,
                ClientNip = i.ClientNip,

                Client = (i.ClientId.HasValue && clientsDict.TryGetValue(i.ClientId.Value, out var c))
                    ? new Client { ClientId = c.ClientId, Name = c.Name, Nip = c.Nip }
                    : null!,

                InvoicePositions = [.. positionsLookup[i.InvoiceId].Select(ip => new InvoicePosition
                {
                    InvoicePositionId = ip.InvoicePositionId,
                    InvoiceId = ip.InvoiceId,
                    ProductId = ip.ProductId,
                    ProductName = ip.ProductName,
                    ProductValue = ip.ProductValue,
                    Quantity = ip.Quantity
                })]
            })];
        }

        public async Task<Product> GetProductAsync(string name, string description, decimal? value, int userId, CancellationToken cancellationToken)
        {
            var product = await _db.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p =>
                    p.Name == name &&
                    p.Description == description &&
                    p.UserId == userId &&
                    p.IsDeleted == false &&
                    p.Value.HasValue == value.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return null;
            }

            return new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                UserId = product.UserId,
                IsDeleted = product.IsDeleted
            };
        }

        public async Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
        {
            var product = await _db.Set<ProductEntity>().AsNoTracking().SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

            if (product == null)
            {
                return null;
            }
            return new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                UserId = product.UserId,
                IsDeleted = product.IsDeleted
            };
        }

        public Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
                _db.Set<Invoice>()
                .AsNoTracking()
                .AnyAsync(i => i.InvoiceId == invoiceId, cancellationToken);

        public Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
             _db.Set<InvoicePosition>()
            .AsNoTracking()
            .AnyAsync(ip => ip.InvoiceId == invoiceId, cancellationToken);

        public async Task RemoveAsync(Invoice invoiceEntity)
        {
            _db.Set<Invoice>().Remove(invoiceEntity);
            await _db.SaveChangesAsync(CancellationToken.None);
        }

        public async Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition)
        {
            _db.Set<InvoicePosition>().Remove(invoicePosition);
            await _db.SaveChangesAsync(CancellationToken.None);
        }

        public async Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
        {
            _db.Set<InvoicePosition>().RemoveRange(invoicePositions);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}
