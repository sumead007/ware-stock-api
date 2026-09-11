using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    DbSet<ProductCategory> ProductCategories { get; }

    DbSet<ProductUnit> ProductUnits { get; }

    DbSet<StockTransaction> StockTransactions { get; }

    DbSet<UserSettings> UserSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
