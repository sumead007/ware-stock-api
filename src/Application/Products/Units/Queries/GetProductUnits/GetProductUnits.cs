using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;

namespace WareStockApi.Application.Products.Units.Queries.GetProductUnits;

public record GetProductUnitsQuery : IRequest<List<LookupDto>>;

public class GetProductUnitsQueryHandler : IRequestHandler<GetProductUnitsQuery, List<LookupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductUnitsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LookupDto>> Handle(GetProductUnitsQuery request, CancellationToken cancellationToken) =>
        await _context.ProductUnits
            .AsNoTracking()
            .OrderBy(u => u.Label)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
