using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Stock;

public class StockTransactionDto
{
    public string Id { get; init; } = string.Empty;

    public TransactionType Type { get; init; }

    public string ProductId { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public DateOnly Date { get; init; }

    public string? Reference { get; init; }

    public string Counterparty { get; init; } = string.Empty;

    public string PerformedBy { get; init; } = string.Empty;

    public string? Note { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<StockTransaction, StockTransactionDto>();
        }
    }
}
