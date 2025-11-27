namespace CreateInvoiceSystem.Invoices.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CreateInvoiceSystem.Abstractions.Entities;


public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Value)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.PaymentDate)
            .IsRequired();

        builder.Property(i => i.CreatedDate)
            .IsRequired();

        builder.Property(i => i.Comments)
            .HasMaxLength(500);

        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices) 
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.User)
            .WithMany(u => u.Invoices) 
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);        
        
        builder.HasMany(i => i.Products)
            .WithMany();

        builder.Property(i => i.MethodOfPayment)
            .HasMaxLength(50);

        builder.Property(i => i.Product)
            .HasMaxLength(50)
            .IsRequired();
    }
}