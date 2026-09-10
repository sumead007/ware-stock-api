using WareStockApi.Application.Common.Interfaces;

namespace WareStockApi.Application.Integrations.Queries.GetIntegrations;

public enum IntegrationFilterType
{
    All,
    Connected,
    NotConnected
}

public enum SortDirection
{
    Asc,
    Desc
}

public record GetIntegrationsQuery : IRequest<List<IntegrationDto>>
{
    public string? Filter { get; init; }

    public IntegrationFilterType Type { get; init; } = IntegrationFilterType.All;

    public SortDirection Sort { get; init; } = SortDirection.Asc;
}

public class GetIntegrationsQueryHandler : IRequestHandler<GetIntegrationsQuery, List<IntegrationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetIntegrationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<IntegrationDto>> Handle(GetIntegrationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Integrations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.Where(i => i.Name.Contains(request.Filter));
        }

        query = request.Type switch
        {
            IntegrationFilterType.Connected => query.Where(i => i.Connected),
            IntegrationFilterType.NotConnected => query.Where(i => !i.Connected),
            _ => query
        };

        query = request.Sort == SortDirection.Desc
            ? query.OrderByDescending(i => i.Name)
            : query.OrderBy(i => i.Name);

        return await query
            .ProjectTo<IntegrationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
