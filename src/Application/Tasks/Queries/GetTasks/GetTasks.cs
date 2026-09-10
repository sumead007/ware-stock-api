using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Common.Validators;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks.Queries.GetTasks;

public record GetTasksQuery : IRequest<PaginatedList<TaskDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public IReadOnlyCollection<WorkTaskStatus>? Status { get; init; }

    public IReadOnlyCollection<TaskPriority>? Priority { get; init; }

    /// <summary>Global search across id or title.</summary>
    public string? Filter { get; init; }
}

public class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PaginatedList<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkTasks.AsNoTracking().AsQueryable();

        if (request.Status is { Count: > 0 })
        {
            query = query.Where(t => request.Status.Contains(t.Status));
        }

        if (request.Priority is { Count: > 0 })
        {
            query = query.Where(t => request.Priority.Contains(t.Priority));
        }

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.Where(t => t.Id.Contains(request.Filter) || t.Title.Contains(request.Filter));
        }

        return await PaginatedList<TaskDto>.CreateAsync(
            query.OrderByDescending(t => t.Created).ProjectTo<TaskDto>(_mapper.ConfigurationProvider),
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
