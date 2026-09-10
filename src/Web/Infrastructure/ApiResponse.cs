using System.Text.Json.Serialization;
using WareStockApi.Application.Common.Models;

namespace WareStockApi.Web.Infrastructure;

/// <summary>
/// Shared success response envelope. Every successful endpoint response (except <c>DELETE</c>
/// and <c>GET /tasks/export</c>) is wrapped in this shape: <c>{ data, message, status, status_code }</c>.
/// </summary>
public class ApiResponse<T>
{
    public required T Data { get; init; }

    public string Message { get; init; } = "Success";

    public string Status { get; init; } = "success";

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; } = StatusCodes.Status200OK;
}

/// <summary>
/// Success envelope for list endpoints, adding a <see cref="Pagination"/> block alongside <c>data</c>.
/// </summary>
public class ApiListResponse<T> : ApiResponse<IReadOnlyCollection<T>>
{
    public required Pagination Pagination { get; init; }
}

public class Pagination
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; init; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; init; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("total_records")]
    public int TotalRecords { get; init; }

    public static Pagination From<T>(PaginatedList<T> list) => new()
    {
        CurrentPage = list.Page,
        PageSize = list.PageSize,
        TotalPages = list.TotalPages,
        TotalRecords = list.TotalCount
    };
}

/// <summary>
/// Shared error response envelope: <c>{ error_code, errors, message, request_id, status, status_code }</c>.
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("error_code")]
    public required string ErrorCode { get; init; }

    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();

    public required string Message { get; init; }

    [JsonPropertyName("request_id")]
    public required string RequestId { get; init; }

    public string Status { get; init; } = "error";

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }
}

public static class ApiResponseExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this T data, string message = "Success", int statusCode = StatusCodes.Status200OK) =>
        new() { Data = data, Message = message, StatusCode = statusCode };

    public static ApiListResponse<T> ToApiListResponse<T>(this PaginatedList<T> list, string message = "Success", int statusCode = StatusCodes.Status200OK) =>
        new() { Data = list.Items, Pagination = Pagination.From(list), Message = message, StatusCode = statusCode };

    /// <summary>
    /// Wraps a non-paginated list (e.g. lookups, conversations, integrations) in the plain
    /// <c>{ data, message, status, status_code }</c> envelope — no <c>pagination</c> sibling,
    /// per the OpenAPI spec, which only attaches <c>Pagination</c> to true list endpoints.
    /// </summary>
    public static ApiResponse<IReadOnlyCollection<T>> ToApiResponse<T>(this IReadOnlyCollection<T> items, string message = "Success", int statusCode = StatusCodes.Status200OK) =>
        new() { Data = items, Message = message, StatusCode = statusCode };
}
