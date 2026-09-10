namespace WareStockApi.Web.Endpoints;

/// <summary>Shared request/response shapes for the various <c>bulk-*</c> endpoints.</summary>
public record BulkIdsRequest(IReadOnlyCollection<string> Ids);

public record BulkUpdateResult(int Updated);
