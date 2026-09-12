using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;
using FluentValidation.Results;
using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;

namespace WareStockApi.Application.Stock.Commands.CreateStockTransaction;

public record CreateStockTransactionCommand : IRequest<StockTransactionDto>
{
    public TransactionType Type { get; init; }

    public string ProductId { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public DateTime Date { get; init; }

    public string? Reference { get; init; }

    public string Counterparty { get; init; } = string.Empty;

    public string PerformedBy { get; init; } = string.Empty;

    public string? Note { get; init; }
}

public class CreateStockTransactionCommandValidator : AbstractValidator<CreateStockTransactionCommand>
{
    public CreateStockTransactionCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(v => v.Counterparty).NotEmpty();
        RuleFor(v => v.PerformedBy).NotEmpty();
    }
}

public class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, StockTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateStockTransactionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StockTransactionDto> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

        if (product is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.ProductId), "Product not found.")
            ]);
        }

        if (request.Type == TransactionType.Withdraw && request.Quantity > product.Quantity)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Quantity), "Quantity exceeds the current stock on hand.")
            ]);
        }

        product.Quantity += request.Type == TransactionType.Receive ? request.Quantity : -request.Quantity;

        var transaction = new StockTransaction
        {
            Type = request.Type,
            ProductId = product.Id,
            ProductName = product.Name,
            Sku = product.Sku,
            Quantity = request.Quantity,
            Date = request.Date,
            Reference = request.Reference,
            Counterparty = request.Counterparty,
            PerformedBy = request.PerformedBy,
            Note = request.Note
        };

        _context.StockTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockTransactionDto>(transaction);
    }
}
