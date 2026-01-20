using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Products.Persistence.Configuration;
public class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).UseIdentityColumn();
        builder.Property(p => p.ProductId)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,2)");
    }
}