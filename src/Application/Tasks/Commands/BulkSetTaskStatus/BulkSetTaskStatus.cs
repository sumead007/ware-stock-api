using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks.Commands.BulkSetTaskStatus;

public record BulkSetTaskStatusCommand(IReadOnlyCollection<string> Ids, WorkTaskStatus Status) : IRequest<int>;

public class BulkSetTaskStatusCommandValidator : AbstractValidator<BulkSetTaskStatusCommand>
{
    public BulkSetTaskStatusCommandValidator()
    {
        RuleFor(v => v.Ids).NotEmpty();
        RuleFor(v => v.Status).IsInEnum();
    }
}

public class BulkSetTaskStatusCommandHandler : IRequestHandler<BulkSetTaskStatusCommand, int>
{
    private readonly IApplicationDbContext _context;

    public BulkSetTaskStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(BulkSetTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var entities = await _context.WorkTasks
            .Where(t => request.Ids.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.Status = request.Status;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entities.Count;
    }
}
