using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Common.Validators;

namespace WareStockApi.Application.Reports.Queries.GetStockLevelsReport;

public record GetStockLevelsReportQuery : IRequest<PaginatedList<StockLevelDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public IReadOnlyCollection<string>? Category { get; init; }

    public string? Sku { get; init; }

    public bool? LowStockOnly { get; init; }

    public DateOnly? UpdatedFrom { get; init; }

    public DateOnly? UpdatedTo { get; init; }
}

public class GetStockLevelsReportQueryValidator : AbstractValidator<GetStockLevelsReportQuery>
{
    public GetStockLevelsReportQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).ValidPageSize();
        RuleFor(q => q.UpdatedFrom)
            .LessThanOrEqualTo(q => q.UpdatedTo)
            .When(q => q.UpdatedFrom.HasValue && q.UpdatedTo.HasValue)
            .WithMessage("'UpdatedFrom' must be on or before 'UpdatedTo'.");
    }
}

public class GetStockLevelsReportQueryHandler : IRequestHandler<GetStockLevelsReportQuery, PaginatedList<StockLevelDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStockLevelsReportQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<StockLevelDto>> Handle(GetStockLevelsReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (request.Category is { Count: > 0 })
        {
            query = query.Where(p => request.Category.Contains(p.Category));
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            query = query.Where(p => p.Sku.Contains(request.Sku));
        }

        if (request.LowStockOnly == true)
        {
            query = query.Where(p => p.Quantity <= p.MinStock);
        }

        if (request.UpdatedFrom.HasValue)
        {
            var from = new DateTimeOffset(request.UpdatedFrom.Value.ToDateTime(TimeOnly.MinValue));
            query = query.Where(p => p.LastModified >= from);
        }

        if (request.UpdatedTo.HasValue)
        {
            var to = new DateTimeOffset(request.UpdatedTo.Value.ToDateTime(TimeOnly.MaxValue));
            query = query.Where(p => p.LastModified <= to);
        }

        return await PaginatedList<StockLevelDto>.CreateAsync(
            query.OrderBy(p => p.Sku).ProjectTo<StockLevelDto>(_mapper.ConfigurationProvider),
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
