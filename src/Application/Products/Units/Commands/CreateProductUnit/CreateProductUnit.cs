using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Units.Commands.CreateProductUnit;

public record CreateProductUnitCommand(string Label) : IRequest<LookupDto>;

public class CreateProductUnitCommandValidator : AbstractValidator<CreateProductUnitCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateProductUnitCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Label).NotEmpty().MaximumLength(50)
            .MustAsync(BeUnique).WithMessage("'{PropertyName}' must be unique.").WithErrorCode("Unique");
    }

    private async Task<bool> BeUnique(string label, CancellationToken cancellationToken) =>
        !await _context.ProductUnits.AnyAsync(u => u.Label == label, cancellationToken);
}

public class CreateProductUnitCommandHandler : IRequestHandler<CreateProductUnitCommand, LookupDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateProductUnitCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LookupDto> Handle(CreateProductUnitCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProductUnit { Label = request.Label };

        _context.ProductUnits.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LookupDto>(entity);
    }
}
