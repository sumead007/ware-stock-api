namespace WareStockApi.Domain.Entities;

/// <summary>
/// Named "WorkTask" (not "Task") to avoid colliding with <see cref="System.Threading.Tasks.Task"/>,
/// which is implicitly in scope everywhere via ImplicitUsings.
/// </summary>
public class WorkTask : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public WorkTaskStatus Status { get; set; }

    public TaskLabel Label { get; set; }

    public TaskPriority Priority { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public string? AssigneeId { get; set; }
}
