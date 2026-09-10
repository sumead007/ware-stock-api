using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Products.Units.Commands.CreateProductUnit;
using WareStockApi.Application.Products.Units.Queries.GetProductUnits;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class ProductUnits : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/product-units";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetProductUnits);
        groupBuilder.MapPost(CreateProductUnit);
    }

    [EndpointSummary("List product units")]
    public static async Task<Ok<ApiResponse<IReadOnlyCollection<LookupDto>>>> GetProductUnits(ISender sender)
    {
        var units = await sender.Send(new GetProductUnitsQuery());

        return TypedResults.Ok(((IReadOnlyCollection<LookupDto>)units).ToApiResponse());
    }

    [EndpointSummary("Create a product unit")]
    public static async Task<Created<ApiResponse<LookupDto>>> CreateProductUnit(ISender sender, CreateProductUnitRequest request)
    {
        var unit = await sender.Send(new CreateProductUnitCommand(request.Label));

        return TypedResults.Created(
            (string?)null, unit.ToApiResponse("Unit created.", StatusCodes.Status201Created));
    }

    public record CreateProductUnitRequest(string Label);
}
