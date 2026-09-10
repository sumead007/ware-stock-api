namespace WareStockApi.Domain.Entities;

/// <summary>
/// Lookup table backing the creatable "unit" combobox in the Products UI.
/// </summary>
public class ProductUnit : BaseEntity
{
    public string Label { get; set; } = string.Empty;
}
