using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Products;
using WareStockApi.Application.Stock;

namespace WareStockApi.Application.Dashboard;

public class DashboardSummaryDto
{
    public required InventorySummaryDto Inventory { get; init; }

    public required UsersSummaryDto Users { get; init; }
}

public class InventorySummaryDto
{
    public int TotalSkuCount { get; init; }

    public decimal TotalQuantity { get; init; }

    public decimal TotalStockValue { get; init; }

    public int LowStockCount { get; init; }

    public required IReadOnlyList<ProductDto> LowStockItems { get; init; }

    public required IReadOnlyList<DailyTransactionTrendDto> DailyTransactionTrend { get; init; }

    public required IReadOnlyList<StockTransactionDto> RecentTransactions { get; init; }
}

public class DailyTransactionTrendDto
{
    public DateOnly Date { get; init; }

    public decimal Receive { get; init; }

    public decimal Withdraw { get; init; }
}

public class UsersSummaryDto
{
    public int Total { get; init; }

    public int Active { get; init; }

    public int Suspended { get; init; }

    public required IReadOnlyList<UserDto> RecentUsers { get; init; }
}
