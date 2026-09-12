using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Reports;
using WareStockApi.Application.Reports.Queries.GetStockLevelsReport;
using WareStockApi.Application.Stock;
using WareStockApi.Application.Stock.Queries.GetStockTransactions;
using WareStockApi.Application.Users.Queries.GetUsers;
using WareStockApi.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Reports : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/reports";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetStockLevelsReport, "stock-levels");
        groupBuilder.MapGet(GetMovementsReport, "movements");
        groupBuilder.MapGet(GetUsersReport, "users");
    }

    [EndpointSummary("Stock levels report")]
    public static async Task<Ok<ApiListResponse<StockLevelDto>>> GetStockLevelsReport(
        ISender sender, int page = 1, int pageSize = 10, string[]? category = null, string? sku = null, bool? lowStockOnly = null,
        DateOnly? updatedFrom = null, DateOnly? updatedTo = null)
    {
        var result = await sender.Send(new GetStockLevelsReportQuery
        {
            Page = page,
            PageSize = pageSize,
            Category = category,
            Sku = sku,
            LowStockOnly = lowStockOnly,
            UpdatedFrom = updatedFrom,
            UpdatedTo = updatedTo
        });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Stock movements report")]
    public static async Task<Ok<ApiListResponse<StockTransactionDto>>> GetMovementsReport(
        ISender sender, int page = 1, int pageSize = 10, string[]? type = null, string? sku = null, DateOnly? from = null, DateOnly? to = null)
    {
        // Minimal API's default enum query binding is case-sensitive against the C#
        // member name (e.g. "Receive"), but the response body serializes enums as
        // camelCase ("receive") per JsonStringEnumConverter(CamelCase) below — parse
        // case-insensitively here so the same casing the API returns can be filtered on.
        var parsedTypes = type?
            .Select(t => Enum.TryParse<TransactionType>(t, ignoreCase: true, out var value) ? value : (TransactionType?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToArray();

        var result = await sender.Send(new GetStockTransactionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Type = parsedTypes,
            Sku = sku,
            From = from,
            To = to
        });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Users report")]
    public static async Task<Ok<ApiListResponse<UserDto>>> GetUsersReport(
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

        var result = await sender.Send(new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = parsedStatuses,
            Username = username,
            From = from,
            To = to
        });

        return TypedResults.Ok(result.ToApiListResponse());
    }
}
