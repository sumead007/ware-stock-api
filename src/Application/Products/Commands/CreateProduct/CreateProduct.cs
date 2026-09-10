using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Commands.CreateProduct;

public record CreateProductCommand : IRequest<ProductDto>
{
    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal MinStock { get; init; }

    public string Location { get; init; } = string.Empty;

    public decimal? CostPrice { get; init; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Sku).NotEmpty().MaximumLength(100)
            .MustAsync(BeUniqueSku).WithMessage("'{PropertyName}' must be unique.").WithErrorCode("Unique");
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Category).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Unit).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Location).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(v => v.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(v => v.CostPrice).GreaterThanOrEqualTo(0).When(v => v.CostPrice is not null);
    }

    private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken) =>
        !await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Category = request.Category,
            Unit = request.Unit,
            Quantity = request.Quantity,
            MinStock = request.MinStock,
            Location = request.Location,
            CostPrice = request.CostPrice
        };

        _context.Products.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(entity);
    }
}
