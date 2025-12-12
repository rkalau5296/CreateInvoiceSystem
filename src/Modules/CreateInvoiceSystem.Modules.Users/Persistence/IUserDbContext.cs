using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Users.Persistence
{
    public interface IUserDbContext : ISaveChangesContext
    {
        public DbSet<User> Users { get; set; }
    }
}
