using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(string Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = Ensure.Found(
            await _context.Products.FindAsync([request.Id], cancellationToken),
            nameof(Product),
            request.Id);

        _context.Products.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
