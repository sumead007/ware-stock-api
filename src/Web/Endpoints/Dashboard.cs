using WareStockApi.Application.Dashboard;
using WareStockApi.Application.Dashboard.Queries.GetDashboardSummary;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Dashboard : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/dashboard";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetSummary, "summary");
    }

    [EndpointSummary("Dashboard summary")]
    public static async Task<Ok<ApiResponse<DashboardSummaryDto>>> GetSummary(ISender sender)
    {
        var summary = await sender.Send(new GetDashboardSummaryQuery());

        return TypedResults.Ok(summary.ToApiResponse());
    }
}
