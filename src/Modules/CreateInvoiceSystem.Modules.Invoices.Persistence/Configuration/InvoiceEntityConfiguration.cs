using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Configuration;

public class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);
        builder.HasIndex(i => new { i.UserId, i.Title }).IsUnique();
        builder.Property(i => i.InvoiceId).ValueGeneratedOnAdd();
        builder.Property(i => i.TotalNet).HasPrecision(38, 2);
        builder.Property(i => i.TotalVat).HasPrecision(38, 2);
        builder.Property(i => i.TotalGross).HasPrecision(38, 2);
        builder.Property(i => i.Title).HasMaxLength(250);
        builder.Property(i => i.MethodOfPayment).HasMaxLength(100);
        builder.Property(i => i.SellerName).HasMaxLength(200).IsRequired(false); ;
        builder.Property(i => i.SellerNip).HasMaxLength(50).IsRequired(false); ;
        builder.Property(i => i.SellerAddress).HasMaxLength(500).IsRequired(false);
        builder.Property(i => i.BankAccountNumber).HasMaxLength(64).IsRequired(false);
        builder.Property(i => i.ClientName).HasMaxLength(200);
        builder.Property(i => i.ClientAddress).HasMaxLength(500);
        builder.Property(i => i.ClientNip).HasMaxLength(50);
        builder.Property(i => i.ClientEmail).HasMaxLength(250).IsRequired(false);
        builder.Property(i => i.Comments).IsRequired(false);
        builder.Property(i => i.ClientId).IsRequired(false);
        builder.Property(i => i.UserId).IsRequired();        
    }
}