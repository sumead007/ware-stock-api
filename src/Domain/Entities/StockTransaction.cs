namespace WareStockApi.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public TransactionType Type { get; set; }

    public string ProductId { get; set; } = string.Empty;

    /// <summary>Snapshot of <see cref="Product.Name"/> at the time the transaction was recorded.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Snapshot of <see cref="Product.Sku"/> at the time the transaction was recorded.</summary>
    public string Sku { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public DateTime Date { get; set; }

    public string? Reference { get; set; }

    /// <summary>Supplier (receive) or requesting department/person (withdraw).</summary>
    public string Counterparty { get; set; } = string.Empty;

    public string PerformedBy { get; set; } = string.Empty;

    public string? Note { get; set; }
}
