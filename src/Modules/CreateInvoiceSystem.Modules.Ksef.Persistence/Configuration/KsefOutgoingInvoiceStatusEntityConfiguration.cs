using CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Ksef.Persistence.Configuration;

public class KsefOutgoingInvoiceStatusEntityConfiguration : IEntityTypeConfiguration<KsefOutgoingInvoiceStatus>
{
    public void Configure(EntityTypeBuilder<KsefOutgoingInvoiceStatus> builder)
    {
        builder.ToTable("KsefOutgoingInvoiceStatuses");

        builder.HasKey(k => k.KsefOutgoingInvoiceStatusId);
        builder.Property(k => k.KsefOutgoingInvoiceStatusId).ValueGeneratedOnAdd();

        builder.HasIndex(k => k.InvoiceId).IsUnique();
        builder.Property(k => k.InvoiceId).IsRequired();

        builder.Property(k => k.Status).IsRequired();

        builder.Property(k => k.ElementReferenceNumber).HasMaxLength(100).IsRequired(false);
        builder.Property(k => k.KsefReferenceNumber).HasMaxLength(100).IsRequired(false);
        builder.Property(k => k.UpoXml).IsRequired(false);
        builder.Property(k => k.ErrorMessage).HasMaxLength(1000).IsRequired(false);

        builder.Property(k => k.SentAt).IsRequired();
        builder.Property(k => k.ConfirmedAt).IsRequired(false);
    }
}