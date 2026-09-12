using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Products;
using WareStockApi.Application.Stock;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context, IIdentityService identityService, IMapper mapper, TimeProvider timeProvider)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products.AsNoTracking().ToListAsync(cancellationToken);

        var totalSkuCount = products.Count;
        var totalQuantity = products.Sum(p => p.Quantity);
        var totalStockValue = products.Sum(p => p.Quantity * (p.CostPrice ?? 0));
        var lowStockProducts = products.Where(p => p.Quantity <= p.MinStock).OrderBy(p => p.Quantity).ToList();

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var startDate = today.AddDays(-6);
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);

        var recentTransactionEntities = await _context.StockTransactions
            .AsNoTracking()
            .Where(t => t.Date >= startDateTime)
            .ToListAsync(cancellationToken);

        var trend = Enumerable.Range(0, 7)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new DailyTransactionTrendDto
            {
                Date = date,
                Receive = recentTransactionEntities.Where(t => DateOnly.FromDateTime(t.Date) == date && t.Type == TransactionType.Receive).Sum(t => t.Quantity),
                Withdraw = recentTransactionEntities.Where(t => DateOnly.FromDateTime(t.Date) == date && t.Type == TransactionType.Withdraw).Sum(t => t.Quantity)
            })
            .ToList();

        var recentTransactions = await _context.StockTransactions
            .AsNoTracking()
            .OrderByDescending(t => t.Date)
            .Take(5)
            .ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var userCounts = await _identityService.GetUserCountsAsync(cancellationToken);
        var recentUsers = await _identityService.GetRecentUsersAsync(5, cancellationToken);

        return new DashboardSummaryDto
        {
            Inventory = new InventorySummaryDto
            {
                TotalSkuCount = totalSkuCount,
                TotalQuantity = totalQuantity,
                TotalStockValue = totalStockValue,
                LowStockCount = lowStockProducts.Count,
                LowStockItems = _mapper.Map<List<ProductDto>>(lowStockProducts),
                DailyTransactionTrend = trend,
                RecentTransactions = recentTransactions
            },
            Users = new UsersSummaryDto
            {
                Total = userCounts.Total,
                Active = userCounts.Active,
                Suspended = userCounts.Suspended,
                RecentUsers = recentUsers.ToList()
            }
        };
    }
}
