using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Tasks;

public class TaskDto
{
    public string Id { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public WorkTaskStatus Status { get; init; }

    public TaskLabel Label { get; init; }

    public TaskPriority Priority { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? DueDate { get; init; }

    public string? AssigneeId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkTask, TaskDto>()
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.Created))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.LastModified));
        }
    }
}
