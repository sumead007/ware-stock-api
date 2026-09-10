using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<WorkTask> WorkTasks { get; }

    DbSet<Product> Products { get; }

    DbSet<ProductCategory> ProductCategories { get; }

    DbSet<ProductUnit> ProductUnits { get; }

    DbSet<StockTransaction> StockTransactions { get; }

    DbSet<Conversation> Conversations { get; }

    DbSet<Message> Messages { get; }

    DbSet<Integration> Integrations { get; }

    DbSet<UserSettings> UserSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
