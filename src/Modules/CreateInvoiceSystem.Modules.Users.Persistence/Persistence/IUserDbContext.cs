using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Users.Persistence.Persistence
{
    public interface IUserDbContext : ISaveChangesContext
    {
        DbSet<UserEntity> Users { get; set; }
    }
}
