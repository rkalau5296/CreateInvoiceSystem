using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CreateInvoiceSystem.Modules.Users.Domain.Configuration;
public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(100);
        
        //builder.HasMany(u => u.Clients)
        //    .WithOne(c => c.User)
        //    .HasForeignKey(c => c.UserId)
        //    .OnDelete(DeleteBehavior.NoAction);
        //builder.HasMany(u => u.Products)
        //    .WithOne(p => p.User)
        //    .HasForeignKey(p => p.UserId)
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.HasMany(u => u.Invoices)
        //        .WithOne(i => i.User)
        //        .HasForeignKey(i => i.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //builder.HasMany(u => u.MethodOfPayments)
        //        .WithOne(i => i.User)
        //        .HasForeignKey(i => i.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);


    }
}
