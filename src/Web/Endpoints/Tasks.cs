using WareStockApi.Application.Tasks;
using WareStockApi.Application.Tasks.Commands.BulkDeleteTasks;
using WareStockApi.Application.Tasks.Commands.BulkSetTaskPriority;
using WareStockApi.Application.Tasks.Commands.BulkSetTaskStatus;
using WareStockApi.Application.Tasks.Commands.CreateTask;
using WareStockApi.Application.Tasks.Commands.DeleteTask;
using WareStockApi.Application.Tasks.Commands.ImportTasksCsv;
using WareStockApi.Application.Tasks.Commands.UpdateTask;
using WareStockApi.Application.Tasks.Queries.ExportTasksCsv;
using WareStockApi.Application.Tasks.Queries.GetTaskById;
using WareStockApi.Application.Tasks.Queries.GetTasks;
using WareStockApi.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Tasks : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/tasks";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetTasks);
        groupBuilder.MapPost(CreateTask);
        groupBuilder.MapGet(ExportTasksCsv, "export");
        groupBuilder.MapPost(ImportTasksCsv, "import");
        groupBuilder.MapGet(GetTaskById, "{id}");
        groupBuilder.MapPut(UpdateTask, "{id}");
        groupBuilder.MapDelete(DeleteTask, "{id}");
        groupBuilder.MapPost(BulkDeleteTasks, "bulk-delete");
        groupBuilder.MapPost(BulkSetTaskStatus, "bulk-status");
        groupBuilder.MapPost(BulkSetTaskPriority, "bulk-priority");
    }

    [EndpointSummary("List tasks")]
    public static async Task<Ok<ApiListResponse<TaskDto>>> GetTasks(
        ISender sender, int page = 1, int pageSize = 10, string[]? status = null, TaskPriority[]? priority = null, string? filter = null)
    {
        var parsedStatuses = status?
            .Select(s => TaskEnumFormatter.TryParseStatus(s, out var parsed) ? (WorkTaskStatus?)parsed : null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        var result = await sender.Send(new GetTasksQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = parsedStatuses,
            Priority = priority,
            Filter = filter
        });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Create a task")]
    public static async Task<Created<ApiResponse<TaskDto>>> CreateTask(ISender sender, CreateTaskCommand command)
    {
        var task = await sender.Send(command);

        return TypedResults.Created($"/v1/tasks/{task.Id}", task.ToApiResponse("Task created.", StatusCodes.Status201Created));
    }

    [EndpointSummary("Get a task by id")]
    public static async Task<Ok<ApiResponse<TaskDto>>> GetTaskById(ISender sender, string id)
    {
        var task = await sender.Send(new GetTaskByIdQuery(id));

        return TypedResults.Ok(task.ToApiResponse());
    }

    [EndpointSummary("Update a task")]
    public static async Task<Ok<ApiResponse<TaskDto>>> UpdateTask(ISender sender, string id, UpdateTaskCommand command)
    {
        var task = await sender.Send(command with { Id = id });

        return TypedResults.Ok(task.ToApiResponse("Task updated."));
    }

    [EndpointSummary("Delete a task")]
    public static async Task<NoContent> DeleteTask(ISender sender, string id)
    {
        await sender.Send(new DeleteTaskCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Bulk delete tasks")]
    public static async Task<NoContent> BulkDeleteTasks(ISender sender, BulkIdsRequest request)
    {
        await sender.Send(new BulkDeleteTasksCommand(request.Ids));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Bulk update task status")]
    public static async Task<Ok<ApiResponse<BulkUpdateResult>>> BulkSetTaskStatus(ISender sender, BulkSetTaskStatusRequest request)
    {
        var updated = await sender.Send(new BulkSetTaskStatusCommand(request.Ids, request.Status));

        return TypedResults.Ok(new BulkUpdateResult(updated).ToApiResponse("Status updated."));
    }

    [EndpointSummary("Bulk update task priority")]
    public static async Task<Ok<ApiResponse<BulkUpdateResult>>> BulkSetTaskPriority(ISender sender, BulkSetTaskPriorityRequest request)
    {
        var updated = await sender.Send(new BulkSetTaskPriorityCommand(request.Ids, request.Priority));

        return TypedResults.Ok(new BulkUpdateResult(updated).ToApiResponse("Priority updated."));
    }

    [EndpointSummary("Export tasks as CSV")]
    [EndpointDescription("Returns a raw text/csv file — not wrapped in the JSON response envelope.")]
    public static async Task<ContentHttpResult> ExportTasksCsv(ISender sender, string[] ids)
    {
        var csv = await sender.Send(new ExportTasksCsvQuery(ids));

        return TypedResults.Text(csv, "text/csv");
    }

    [EndpointSummary("Import tasks from CSV")]
    public static async Task<Ok<ApiResponse<ImportTasksCsvResult>>> ImportTasksCsv(ISender sender, IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var result = await sender.Send(new ImportTasksCsvCommand(content));

        return TypedResults.Ok(result.ToApiResponse("Import completed."));
    }

    public record BulkSetTaskStatusRequest(IReadOnlyCollection<string> Ids, WorkTaskStatus Status);
    public record BulkSetTaskPriorityRequest(IReadOnlyCollection<string> Ids, TaskPriority Priority);
}
