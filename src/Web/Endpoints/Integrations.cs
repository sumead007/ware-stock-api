using WareStockApi.Application.Integrations;
using WareStockApi.Application.Integrations.Queries.GetIntegrations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Integrations : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/integrations";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetIntegrations);
    }

    [EndpointSummary("List integrations")]
    public static async Task<Ok<ApiResponse<IReadOnlyCollection<IntegrationDto>>>> GetIntegrations(
        ISender sender, string? filter = null, IntegrationFilterType type = IntegrationFilterType.All, SortDirection sort = SortDirection.Asc)
    {
        var integrations = await sender.Send(new GetIntegrationsQuery { Filter = filter, Type = type, Sort = sort });

        return TypedResults.Ok(((IReadOnlyCollection<IntegrationDto>)integrations).ToApiResponse());
    }
}
