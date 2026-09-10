using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Categories.Commands.CreateProductCategory;

public record CreateProductCategoryCommand(string Label) : IRequest<LookupDto>;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCategoryCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Label).NotEmpty().MaximumLength(100)
            .MustAsync(BeUnique).WithMessage("'{PropertyName}' must be unique.").WithErrorCode("Unique");
    }

    private async Task<bool> BeUnique(string label, CancellationToken cancellationToken) =>
        !await _context.ProductCategories.AnyAsync(c => c.Label == label, cancellationToken);
}

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, LookupDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateProductCategoryCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LookupDto> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProductCategory { Label = request.Label };

        _context.ProductCategories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LookupDto>(entity);
    }
}
