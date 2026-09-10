using WareStockApi.Application.Stock;
using WareStockApi.Application.Stock.Commands.CreateStockTransaction;
using WareStockApi.Application.Stock.Queries.GetStockTransactions;
using WareStockApi.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class StockTransactions : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/stock-transactions";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetStockTransactions);
        groupBuilder.MapPost(CreateStockTransaction);
    }

    [EndpointSummary("List stock transactions")]
    public static async Task<Ok<ApiListResponse<StockTransactionDto>>> GetStockTransactions(
        ISender sender, int page = 1, int pageSize = 10, TransactionType[]? type = null, string? sku = null)
    {
        var result = await sender.Send(new GetStockTransactionsQuery { Page = page, PageSize = pageSize, Type = type, Sku = sku });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Record a stock transaction")]
    [EndpointDescription("Records a receive/withdraw transaction. Withdrawals that exceed the current stock on hand return 400.")]
    public static async Task<Created<ApiResponse<StockTransactionDto>>> CreateStockTransaction(ISender sender, CreateStockTransactionCommand command)
    {
        var transaction = await sender.Send(command);

        return TypedResults.Created(
            $"/v1/stock-transactions/{transaction.Id}", transaction.ToApiResponse("Transaction recorded.", StatusCodes.Status201Created));
    }
}
