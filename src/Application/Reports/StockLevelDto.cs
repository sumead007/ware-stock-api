using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Reports;

public class StockLevelDto
{
    public string Id { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal MinStock { get; init; }

    public string Location { get; init; } = string.Empty;

    public decimal? CostPrice { get; init; }

    public decimal CostValue { get; init; }

    public bool IsLowStock { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, StockLevelDto>()
                .ForMember(d => d.CostValue, opt => opt.MapFrom(s => s.Quantity * (s.CostPrice ?? 0)))
                .ForMember(d => d.IsLowStock, opt => opt.MapFrom(s => s.Quantity <= s.MinStock))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.LastModified));
        }
    }
}
