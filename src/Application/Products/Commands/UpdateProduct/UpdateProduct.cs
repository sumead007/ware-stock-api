using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<ProductDto>
{
    public string Id { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal MinStock { get; init; }

    public string Location { get; init; } = string.Empty;

    public decimal? CostPrice { get; init; }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandValidator(IApplicationDbContext context)
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

    private async Task<bool> BeUniqueSku(UpdateProductCommand command, string sku, CancellationToken cancellationToken) =>
        !await _context.Products.AnyAsync(p => p.Sku == sku && p.Id != command.Id, cancellationToken);
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = Ensure.Found(
            await _context.Products.FindAsync([request.Id], cancellationToken),
            nameof(Product),
            request.Id);

        entity.Sku = request.Sku;
        entity.Name = request.Name;
        entity.Category = request.Category;
        entity.Unit = request.Unit;
        entity.Quantity = request.Quantity;
        entity.MinStock = request.MinStock;
        entity.Location = request.Location;
        entity.CostPrice = request.CostPrice;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(entity);
    }
}
