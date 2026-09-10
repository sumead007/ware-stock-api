using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand : IRequest<TaskDto>
{
    public string Id { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public WorkTaskStatus Status { get; init; }

    public TaskLabel Label { get; init; }

    public TaskPriority Priority { get; init; }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(500);
        RuleFor(v => v.Status).IsInEnum();
        RuleFor(v => v.Label).IsInEnum();
        RuleFor(v => v.Priority).IsInEnum();
    }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = Ensure.Found(
            await _context.WorkTasks.FindAsync([request.Id], cancellationToken),
            nameof(WorkTask),
            request.Id);

        entity.Title = request.Title;
        entity.Status = request.Status;
        entity.Label = request.Label;
        entity.Priority = request.Priority;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(entity);
    }
}
