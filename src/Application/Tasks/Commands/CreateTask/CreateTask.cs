using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks.Commands.CreateTask;

public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;

    public WorkTaskStatus Status { get; init; }

    public TaskLabel Label { get; init; }

    public TaskPriority Priority { get; init; }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(500);
        RuleFor(v => v.Status).IsInEnum();
        RuleFor(v => v.Label).IsInEnum();
        RuleFor(v => v.Priority).IsInEnum();
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkTask
        {
            Title = request.Title,
            Status = request.Status,
            Label = request.Label,
            Priority = request.Priority
        };

        _context.WorkTasks.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(entity);
    }
}
