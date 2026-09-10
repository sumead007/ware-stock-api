using WareStockApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WareStockApi.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Location).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(p => p.MinStock).HasColumnType("decimal(18,2)");
        builder.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.Sku).IsUnique();
    }
}
