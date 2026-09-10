using WareStockApi.Application.Products;
using WareStockApi.Application.Products.Commands.CreateProduct;
using WareStockApi.Application.Products.Commands.DeleteProduct;
using WareStockApi.Application.Products.Commands.UpdateProduct;
using WareStockApi.Application.Products.Queries.GetProductById;
using WareStockApi.Application.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WareStockApi.Web.Endpoints;

public class Products : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/products";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetProducts);
        groupBuilder.MapPost(CreateProduct);
        groupBuilder.MapGet(GetProductById, "{id}");
        groupBuilder.MapPut(UpdateProduct, "{id}");
        groupBuilder.MapDelete(DeleteProduct, "{id}");
    }

    [EndpointSummary("List products")]
    public static async Task<Ok<ApiListResponse<ProductDto>>> GetProducts(
        ISender sender, int page = 1, int pageSize = 10, string[]? category = null, string? sku = null)
    {
        var result = await sender.Send(new GetProductsQuery { Page = page, PageSize = pageSize, Category = category, Sku = sku });

        return TypedResults.Ok(result.ToApiListResponse());
    }

    [EndpointSummary("Create a product")]
    public static async Task<Created<ApiResponse<ProductDto>>> CreateProduct(ISender sender, CreateProductCommand command)
    {
        var product = await sender.Send(command);

        return TypedResults.Created($"/v1/products/{product.Id}", product.ToApiResponse("Product created.", StatusCodes.Status201Created));
    }

    [EndpointSummary("Get a product by id")]
    public static async Task<Ok<ApiResponse<ProductDto>>> GetProductById(ISender sender, string id)
    {
        var product = await sender.Send(new GetProductByIdQuery(id));

        return TypedResults.Ok(product.ToApiResponse());
    }

    [EndpointSummary("Update a product")]
    public static async Task<Ok<ApiResponse<ProductDto>>> UpdateProduct(ISender sender, string id, UpdateProductCommand command)
    {
        var product = await sender.Send(command with { Id = id });

        return TypedResults.Ok(product.ToApiResponse("Product updated."));
    }

    [EndpointSummary("Delete a product")]
    public static async Task<NoContent> DeleteProduct(ISender sender, string id)
    {
        await sender.Send(new DeleteProductCommand(id));

        return TypedResults.NoContent();
    }
}
