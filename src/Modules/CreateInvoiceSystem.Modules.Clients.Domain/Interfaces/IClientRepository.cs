using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
public interface IClientRepository : ISaveChangesContext
{
    Task<bool> ExistsAsync(string name, string street, string number, string city, string postalCode,
        string country, int userId, CancellationToken cancellationToken);    
    Task<List<Client>> GetAllAsync(int? userId, CancellationToken cancellationToken);
    Task<Client> GetByIdAsync(int clientId, int? userId, CancellationToken cancellationToken);
    Task<Client> AddAsync(Client entity, CancellationToken cancellationToken);
    Task<Client> UpdateAsync(Client entity, CancellationToken cancellationToken);    
    Task RemoveAsync(int clientId, CancellationToken cancellationToken);
    Task RemoveAddressAsync(int addressId, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(int clientId, CancellationToken cancellationToken);
    Task<bool> AddressExistsByIdAsync(int addressId, CancellationToken cancellationToken);
      
}