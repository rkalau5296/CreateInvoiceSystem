using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
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
                TotalNet = invoice.TotalNet,
                TotalVat = invoice.TotalVat,
                TotalGross = invoice.TotalGross,
                PaymentDate = invoice.PaymentDate,
                CreatedDate = invoice.CreatedDate,
                Comments = invoice.Comments,
                ClientId = invoice.ClientId,
                UserId = invoice.UserId,
                MethodOfPayment = invoice.MethodOfPayment,
                SellerName = invoice.SellerName,
                SellerNip = invoice.SellerNip,
                SellerAddress = invoice.SellerAddress,
                BankAccountNumber = invoice.BankAccountNumber,
                ClientName = invoice.ClientName,
                ClientAddress = invoice.ClientAddress,
                ClientNip = invoice.ClientNip
            };

            await _db.Set<InvoiceEntity>().AddAsync(invoiceEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var positionsToSave = invoice.InvoicePositions.Select(ip => new InvoicePositionEntity
            {
                InvoiceId = invoiceEntity.InvoiceId,
                ProductId = ip.ProductId,
                ProductName = ip.ProductName,
                ProductDescription = ip.ProductDescription,
                ProductValue = ip.ProductValue,
                Quantity = ip.Quantity,
                VatRate = ip.VatRate
            }).ToList();

            await _db.Set<InvoicePositionEntity>().AddRangeAsync(positionsToSave, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new Invoice
            {
                InvoiceId = invoiceEntity.InvoiceId,
                Title = invoiceEntity.Title,
                TotalNet = invoiceEntity.TotalNet,
                TotalVat = invoiceEntity.TotalVat,
                TotalGross = invoiceEntity.TotalGross,
                PaymentDate = invoiceEntity.PaymentDate,
                CreatedDate = invoiceEntity.CreatedDate,
                Comments = invoiceEntity.Comments,
                ClientId = invoiceEntity.ClientId,
                UserId = invoiceEntity.UserId,
                MethodOfPayment = invoiceEntity.MethodOfPayment,
                SellerName = invoiceEntity.SellerName,
                SellerNip = invoiceEntity.SellerNip,
                SellerAddress = invoiceEntity.SellerAddress,
                BankAccountNumber = invoiceEntity.BankAccountNumber,
                ClientName = invoiceEntity.ClientName,
                ClientAddress = invoiceEntity.ClientAddress,
                ClientNip = invoiceEntity.ClientNip,
                InvoicePositions = positionsToSave.Select(pe => new InvoicePosition
                {
                    InvoicePositionId = pe.InvoicePositionId,
                    InvoiceId = pe.InvoiceId,
                    ProductId = pe.ProductId,
                    ProductName = pe.ProductName,
                    ProductDescription = pe.ProductDescription,
                    ProductValue = pe.ProductValue,
                    Quantity = pe.Quantity,
                    VatRate = pe.VatRate
                }).ToList()
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

        public async Task<Client> GetClientByIdAsync(int? clientId, CancellationToken cancellationToken)
        {
            if (clientId == null)
                throw new ArgumentNullException(nameof(clientId));

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

        public async Task<Invoice> GetInvoiceByIdAsync(int? user, int invoiceId, CancellationToken cancellationToken)
        {
            var invoiceEntity = await _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.InvoiceId == invoiceId && i.UserId == user, cancellationToken)
                ?? throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");

            ClientEntity? clientEntity = null;
            AddressEntity? addressEntity = null;

            if (invoiceEntity.ClientId.HasValue)
            {
                clientEntity = await _db.Set<ClientEntity>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.ClientId == invoiceEntity.ClientId, cancellationToken);

                if (clientEntity != null)
                {
                    addressEntity = await _db.Set<AddressEntity>()
                        .AsNoTracking()
                        .SingleOrDefaultAsync(a => a.AddressId == clientEntity.AddressId, cancellationToken);
                }
            }

            var invoicePositionsEntity = await _db.Set<InvoicePositionEntity>()
                .AsNoTracking()
                .Where(p => p.InvoiceId == invoiceId)
                .ToListAsync(cancellationToken);

            var productIds = invoicePositionsEntity
                .Select(p => p.ProductId)
                .Where(id => id.HasValue)
                .Cast<int>()
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
                TotalNet = invoiceEntity.TotalNet,
                TotalVat = invoiceEntity.TotalVat,
                TotalGross = invoiceEntity.TotalGross,
                PaymentDate = invoiceEntity.PaymentDate,
                CreatedDate = invoiceEntity.CreatedDate,
                Comments = invoiceEntity.Comments,
                ClientId = invoiceEntity.ClientId,
                UserId = invoiceEntity.UserId,
                MethodOfPayment = invoiceEntity.MethodOfPayment,
                SellerName = invoiceEntity.SellerName,
                SellerNip = invoiceEntity.SellerNip,
                SellerAddress = invoiceEntity.SellerAddress,
                BankAccountNumber = invoiceEntity.BankAccountNumber,
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
            VatRate = ip.VatRate,
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

        public async Task<PagedResult<Invoice>> GetInvoicesAsync(int? userId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken)
        {
            var query = _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .Where(i => i.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(i =>
                    i.Title.ToLower().Contains(lowerSearch) ||
                    i.ClientName.ToLower().Contains(lowerSearch) ||
                    i.ClientNip.ToLower().Contains(lowerSearch));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var invoiceEntities = await query
                .OrderByDescending(i => i.InvoiceId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (invoiceEntities.Count == 0)
                return new PagedResult<Invoice>([], totalCount, pageNumber, pageSize);

            var invoiceIds = invoiceEntities.Select(i => i.InvoiceId).ToList();
            var clientIds = invoiceEntities
                    .Select(i => i.ClientId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

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

            var items = invoiceEntities.Select(i => new Invoice
            {
                InvoiceId = i.InvoiceId,
                Title = i.Title,
                TotalNet = i.TotalNet,
                TotalVat = i.TotalVat,
                TotalGross = i.TotalGross,
                PaymentDate = i.PaymentDate,
                CreatedDate = i.CreatedDate,
                Comments = i.Comments,
                ClientId = i.ClientId,
                UserId = i.UserId,
                MethodOfPayment = i.MethodOfPayment,
                SellerName = i.SellerName,
                SellerNip = i.SellerNip,
                SellerAddress = i.SellerAddress,
                BankAccountNumber = i.BankAccountNumber,
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
            ProductDescription = ip.ProductDescription,
            ProductValue = ip.ProductValue,
            Quantity = ip.Quantity,
            VatRate = ip.VatRate
        })]
            }).ToList();

            return new PagedResult<Invoice>(items, totalCount, pageNumber, pageSize);
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

        public Task<bool> InvoiceExistsAsync(int invoiceId, CancellationToken cancellationToken)
        {
            var exists = _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .AnyAsync(i => i.InvoiceId == invoiceId, cancellationToken);
            return exists;
        }

        public Task<bool> InvoicePositionExistsAsync(int invoiceId, CancellationToken cancellationToken) =>
             _db.Set<InvoicePositionEntity>()
            .AsNoTracking()
            .AnyAsync(ip => ip.InvoiceId == invoiceId, cancellationToken);

        public async Task RemoveAsync(Invoice invoice)
        {
            var invoiceEntity = await _db.Set<InvoiceEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId, CancellationToken.None)
                ?? throw new InvalidOperationException($"Invoice with ID {invoice.InvoiceId} not found.");
            _db.Set<InvoiceEntity>().Remove(invoiceEntity);
        }

        public async Task RemoveInvoicePositionsAsync(InvoicePosition invoicePosition)
        {
            var invoicePos = await _db.Set<InvoicePositionEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.InvoicePositionId == invoicePosition.InvoicePositionId, CancellationToken.None)
                ?? throw new InvalidOperationException($"InvoicePosition with ID {invoicePosition.InvoicePositionId} not found.");
            _db.Set<InvoicePositionEntity>().Remove(invoicePos);            
        }

        public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            int? finalClientId = invoice.ClientId;

            if (invoice.ClientId == 0 && invoice.Client != null)
            {
                var addressEntity = new AddressEntity
                {
                    Street = invoice.Client.Address.Street,
                    Number = invoice.Client.Address.Number,
                    City = invoice.Client.Address.City,
                    PostalCode = invoice.Client.Address.PostalCode,
                    Country = invoice.Client.Address.Country
                };

                await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                var newClient = new ClientEntity
                {
                    Name = invoice.Client.Name,
                    Nip = invoice.Client.Nip,
                    AddressId = addressEntity.AddressId,
                    UserId = invoice.UserId
                };

                await _db.Set<ClientEntity>().AddAsync(newClient, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                finalClientId = newClient.ClientId;
            }

            var invoiceEntity = new InvoiceEntity
            {
                InvoiceId = invoice.InvoiceId,
                Title = invoice.Title,
                TotalNet = invoice.TotalNet,
                TotalVat = invoice.TotalVat,
                TotalGross = invoice.TotalGross,
                PaymentDate = invoice.PaymentDate,
                CreatedDate = invoice.CreatedDate,
                Comments = invoice.Comments,
                ClientId = finalClientId,
                UserId = invoice.UserId,
                MethodOfPayment = invoice.MethodOfPayment,
                BankAccountNumber = invoice.BankAccountNumber,
                SellerName = invoice.SellerName,
                SellerNip = invoice.SellerNip,
                SellerAddress = invoice.SellerAddress,
                ClientName = invoice.ClientName,
                ClientAddress = invoice.ClientAddress,
                ClientNip = invoice.ClientNip
            };

            _db.Set<InvoiceEntity>().Update(invoiceEntity);
            await _db.SaveChangesAsync(cancellationToken);

            var positionsToProcess = invoice.InvoicePositions.Select(ip => new InvoicePositionEntity
            {
                InvoicePositionId = ip.InvoicePositionId,
                InvoiceId = invoiceEntity.InvoiceId,
                ProductId = ip.ProductId > 0 ? ip.ProductId : null,
                ProductName = ip.ProductName,
                ProductDescription = ip.ProductDescription,
                ProductValue = ip.ProductValue,
                Quantity = ip.Quantity,
                VatRate = ip.VatRate
            }).ToList();

            foreach (var posEntity in positionsToProcess)
            {
                if (posEntity.InvoicePositionId == 0)
                {
                    await _db.Set<InvoicePositionEntity>().AddAsync(posEntity, cancellationToken);
                }
                else
                {
                    _db.Set<InvoicePositionEntity>().Update(posEntity);
                }
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveRangeAsync(IEnumerable<InvoicePosition> invoicePositions, CancellationToken cancellationToken)
        {
            var invoicePosIds = invoicePositions.Select(ip => ip.InvoicePositionId).ToList();
            var invoicePosEntities = await _db.Set<InvoicePositionEntity>()
                .Where(ip => invoicePosIds.Contains(ip.InvoicePositionId))
                .ToListAsync(cancellationToken);
            _db.Set<InvoicePositionEntity>().RemoveRange(invoicePosEntities);            
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _db.SaveChangesAsync(cancellationToken);

        public async Task<string> GetUserEmailByIdAsync(int userId, CancellationToken ct)
        {
            return await _db.Set<UserEntity>()
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
        {
            var query = from u in _db.Set<UserEntity>()
                        join a in _db.Set<AddressEntity>() on u.AddressId equals a.AddressId 
                        where u.Id == userId
                        select new User
                        {
                            UserId = u.Id,
                            Name = u.Name,
                            CompanyName = u.CompanyName,
                            Nip = u.Nip,
                            AddressId = u.AddressId,
                            BankAccountNumber = u.BankAccountNumber,
                            Address = new Address
                            {
                                AddressId = a.AddressId,
                                Street = a.Street,
                                Number = a.Number,
                                City = a.City,
                                PostalCode = a.PostalCode,
                                Country = a.Country                                
                            }
                        };

            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<int> GetInvoicesCountInMonthAsync(int userId, int month, int year, CancellationToken ct)
        {
            return await _db.Set<InvoiceEntity>()
                .Where(i => i.UserId == userId)
                .Where(i => i.CreatedDate.Month == month && i.CreatedDate.Year == year)
                .CountAsync(ct);
        }

        public async Task RemoveAllByUserIdAsync(int userId, CancellationToken ct)
        {
            var invoices = await _db.Set<InvoiceEntity>()
                .Include(i => i.InvoicePositions)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);

            foreach (var invoice in invoices)
            {
                if (invoice.InvoicePositions != null)
                {                    
                    var positions = invoice.InvoicePositions.OfType<InvoicePositionEntity>();
                    _db.Set<InvoicePositionEntity>().RemoveRange(positions);
                }

                _db.Set<InvoiceEntity>().Remove(invoice);
            }
        }
    }
}
