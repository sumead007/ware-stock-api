using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(string Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking()
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return Ensure.Found(product, nameof(Product), request.Id);
    }
}
