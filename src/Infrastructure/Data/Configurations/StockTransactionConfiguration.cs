using WareStockApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WareStockApi.Infrastructure.Data.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.Property(t => t.ProductId).IsRequired();
        builder.Property(t => t.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Sku).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Counterparty).HasMaxLength(200).IsRequired();
        builder.Property(t => t.PerformedBy).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Quantity).HasColumnType("decimal(18,2)");

        builder.HasIndex(t => t.ProductId);
    }
}
