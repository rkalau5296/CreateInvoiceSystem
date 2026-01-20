using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Configuration;

public class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(100);        

        builder.Property(i => i.PaymentDate)
            .IsRequired();

        builder.Property(i => i.CreatedDate)
            .IsRequired();

        builder.Property(i => i.Comments)
            .HasMaxLength(500);        

        //builder.HasOne(i => i.User)
        //    .WithMany(u => u.Invoices) 
        //    .HasForeignKey(i => i.UserId)
        //    .OnDelete(DeleteBehavior.Restrict);       
       
    }
}