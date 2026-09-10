using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;

namespace WareStockApi.Application.Products.Categories.Queries.GetProductCategories;

public record GetProductCategoriesQuery : IRequest<List<LookupDto>>;

public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery, List<LookupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LookupDto>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken) =>
        await _context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Label)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
