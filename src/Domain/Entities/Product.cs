namespace WareStockApi.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Free-text category name (not a foreign key) — see <see cref="ProductCategory"/>.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Free-text unit name (not a foreign key) — see <see cref="ProductUnit"/>.</summary>
    public string Unit { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal MinStock { get; set; }

    public string Location { get; set; } = string.Empty;

    public decimal? CostPrice { get; set; }
}
