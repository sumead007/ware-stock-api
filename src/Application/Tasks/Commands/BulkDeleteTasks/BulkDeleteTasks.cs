using WareStockApi.Application.Common.Interfaces;

namespace WareStockApi.Application.Tasks.Commands.BulkDeleteTasks;

public record BulkDeleteTasksCommand(IReadOnlyCollection<string> Ids) : IRequest;

public class BulkDeleteTasksCommandValidator : AbstractValidator<BulkDeleteTasksCommand>
{
    public BulkDeleteTasksCommandValidator()
    {
        RuleFor(v => v.Ids).NotEmpty();
    }
}

public class BulkDeleteTasksCommandHandler : IRequestHandler<BulkDeleteTasksCommand>
{
    private readonly IApplicationDbContext _context;

    public BulkDeleteTasksCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(BulkDeleteTasksCommand request, CancellationToken cancellationToken)
    {
        var entities = await _context.WorkTasks
            .Where(t => request.Ids.Contains(t.Id))
            .ToListAsync(cancellationToken);

        _context.WorkTasks.RemoveRange(entities);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
