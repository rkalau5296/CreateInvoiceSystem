using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Configuration;

public class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);
        builder.Property(i => i.InvoiceId).ValueGeneratedOnAdd();

        builder.Property(i => i.TotalNet).HasPrecision(18, 2);
        builder.Property(i => i.TotalVat).HasPrecision(18, 2);
        builder.Property(i => i.TotalGross).HasPrecision(18, 2);

        builder.Property(i => i.Title).HasMaxLength(250);
        builder.Property(i => i.MethodOfPayment).HasMaxLength(100);
        builder.Property(i => i.SellerName).HasMaxLength(200);
        builder.Property(i => i.SellerNip).HasMaxLength(50);
        builder.Property(i => i.SellerAddress).HasMaxLength(500);
        builder.Property(i => i.BankAccountNumber).HasMaxLength(64);
        builder.Property(i => i.ClientName).HasMaxLength(200);
        builder.Property(i => i.ClientAddress).HasMaxLength(500);
        builder.Property(i => i.ClientNip).HasMaxLength(50);

        builder.Ignore(i => i.InvoicePositions);

        builder.Property(i => i.ClientId).IsRequired(false);
        builder.Property(i => i.UserId).IsRequired();
    }
}