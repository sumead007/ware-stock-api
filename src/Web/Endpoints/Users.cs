using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Users.Commands.BulkDeleteUsers;
using WareStockApi.Application.Users.Commands.BulkSetUserStatus;
using WareStockApi.Application.Users.Commands.CreateUser;
using WareStockApi.Application.Users.Commands.DeleteUser;
using WareStockApi.Application.Users.Commands.UpdateUser;
using WareStockApi.Application.Users.Queries.GetUserById;
using WareStockApi.Application.Users.Queries.GetUsers;
using WareStockApi.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/users";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetUsers);
        groupBuilder.MapPost(CreateUser);
        groupBuilder.MapGet(GetUserById, "{id}");
        groupBuilder.MapPut(UpdateUser, "{id}");
        groupBuilder.MapDelete(DeleteUser, "{id}");
        groupBuilder.MapPost(BulkDeleteUsers, "bulk-delete");
        groupBuilder.MapPost(BulkSetUserStatus, "bulk-status");
    }

    [EndpointSummary("List users")]
    public static async Task<Ok<ApiListResponse<UserDto>>> GetUsers(
        ISender sender, int page = 1, int pageSize = 10, string[]? status = null, string? username = null, DateOnly? from = null, DateOnly? to = null)
    {
        // See StockTransactions.GetStockTransactions for why this can't bind as
        // UserStatus[] directly — minimal API's enum query binding is case-sensitive
        // against the C# member name, but responses serialize enums as camelCase.
        var parsedStatuses = status?
            .Select(s => Enum.TryParse<UserStatus>(s, ignoreCase: true, out var value) ? value : (UserStatus?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToArray();

        var result = await sender.Send(new GetUsersQuery { Page = page, PageSize = pageSize, Status = parsedStatuses, Username = username, From = from, To = to });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Create a user")]
    public static async Task<Created<ApiResponse<UserDto>>> CreateUser(ISender sender, CreateUserCommand command)
    {
        var user = await sender.Send(command);

        return TypedResults.Created($"/v1/users/{user.Id}", user.ToApiResponse("User created.", StatusCodes.Status201Created));
    }

    [EndpointSummary("Get a user by id")]
    public static async Task<Ok<ApiResponse<UserDto>>> GetUserById(ISender sender, string id)
    {
        var user = await sender.Send(new GetUserByIdQuery(id));

        return TypedResults.Ok(user.ToApiResponse());
    }

    [EndpointSummary("Update a user")]
    public static async Task<Ok<ApiResponse<UserDto>>> UpdateUser(ISender sender, string id, UpdateUserCommand command)
    {
        var user = await sender.Send(command with { Id = id });

        return TypedResults.Ok(user.ToApiResponse("User updated."));
    }

    [EndpointSummary("Delete a user")]
    public static async Task<NoContent> DeleteUser(ISender sender, string id)
    {
        await sender.Send(new DeleteUserCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Bulk delete users")]
    public static async Task<NoContent> BulkDeleteUsers(ISender sender, BulkIdsRequest request)
    {
        await sender.Send(new BulkDeleteUsersCommand(request.Ids));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Bulk update user status")]
    public static async Task<Ok<ApiResponse<BulkUpdateResult>>> BulkSetUserStatus(ISender sender, BulkSetUserStatusRequest request)
    {
        var updated = await sender.Send(new BulkSetUserStatusCommand(request.Ids, request.Status));

        return TypedResults.Ok(new BulkUpdateResult(updated).ToApiResponse("Status updated."));
    }

    public record BulkSetUserStatusRequest(IReadOnlyCollection<string> Ids, UserStatus Status);
}
