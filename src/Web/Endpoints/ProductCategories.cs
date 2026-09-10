using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Products.Categories.Commands.CreateProductCategory;
using WareStockApi.Application.Products.Categories.Queries.GetProductCategories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class ProductCategories : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/product-categories";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetProductCategories);
        groupBuilder.MapPost(CreateProductCategory);
    }

    [EndpointSummary("List product categories")]
    public static async Task<Ok<ApiResponse<IReadOnlyCollection<LookupDto>>>> GetProductCategories(ISender sender)
    {
        var categories = await sender.Send(new GetProductCategoriesQuery());

        return TypedResults.Ok(((IReadOnlyCollection<LookupDto>)categories).ToApiResponse());
    }

    [EndpointSummary("Create a product category")]
    public static async Task<Created<ApiResponse<LookupDto>>> CreateProductCategory(ISender sender, CreateProductCategoryRequest request)
    {
        var category = await sender.Send(new CreateProductCategoryCommand(request.Label));

        return TypedResults.Created(
            (string?)null, category.ToApiResponse("Category created.", StatusCodes.Status201Created));
    }

    public record CreateProductCategoryRequest(string Label);
}
