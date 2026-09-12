using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Common.Validators;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Stock.Queries.GetStockTransactions;

public record GetStockTransactionsQuery : IRequest<PaginatedList<StockTransactionDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public IReadOnlyCollection<TransactionType>? Type { get; init; }

    public string? Sku { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}

public class GetStockTransactionsQueryValidator : AbstractValidator<GetStockTransactionsQuery>
{
    public GetStockTransactionsQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).ValidPageSize();
        RuleFor(q => q.From)
            .LessThanOrEqualTo(q => q.To)
            .When(q => q.From.HasValue && q.To.HasValue)
            .WithMessage("'From' must be on or before 'To'.");
    }
}

public class GetStockTransactionsQueryHandler : IRequestHandler<GetStockTransactionsQuery, PaginatedList<StockTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStockTransactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<StockTransactionDto>> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StockTransactions.AsNoTracking().AsQueryable();

        if (request.Type is { Count: > 0 })
        {
            query = query.Where(t => request.Type.Contains(t.Type));
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            query = query.Where(t => t.Sku.Contains(request.Sku));
        }

        if (request.From.HasValue)
        {
            var from = request.From.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.Date >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(t => t.Date <= to);
        }

        return await PaginatedList<StockTransactionDto>.CreateAsync(
            query.OrderByDescending(t => t.Date).ProjectTo<StockTransactionDto>(_mapper.ConfigurationProvider),
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
