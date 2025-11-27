namespace CreateInvoiceSystem.Invoices.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CreateInvoiceSystem.Abstractions.Entities;


public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.ProductId);

        //builder.Property(p => p.Name)
        //    .IsRequired()
        //    .HasMaxLength(100);

        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,2)");
    }
}