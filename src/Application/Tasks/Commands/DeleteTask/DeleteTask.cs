using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(string Id) : IRequest;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = Ensure.Found(
            await _context.WorkTasks.FindAsync([request.Id], cancellationToken),
            nameof(WorkTask),
            request.Id);

        _context.WorkTasks.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
