using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks.Commands.BulkSetTaskPriority;

public record BulkSetTaskPriorityCommand(IReadOnlyCollection<string> Ids, TaskPriority Priority) : IRequest<int>;

public class BulkSetTaskPriorityCommandValidator : AbstractValidator<BulkSetTaskPriorityCommand>
{
    public BulkSetTaskPriorityCommandValidator()
    {
        RuleFor(v => v.Ids).NotEmpty();
        RuleFor(v => v.Priority).IsInEnum();
    }
}

public class BulkSetTaskPriorityCommandHandler : IRequestHandler<BulkSetTaskPriorityCommand, int>
{
    private readonly IApplicationDbContext _context;

    public BulkSetTaskPriorityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(BulkSetTaskPriorityCommand request, CancellationToken cancellationToken)
    {
        var entities = await _context.WorkTasks
            .Where(t => request.Ids.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.Priority = request.Priority;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entities.Count;
    }
}
