namespace WareStockApi.Domain.Entities;

/// <summary>
/// Lookup table backing the creatable "category" combobox in the Products UI.
/// </summary>
public class ProductCategory : BaseEntity
{
    public string Label { get; set; } = string.Empty;
}
