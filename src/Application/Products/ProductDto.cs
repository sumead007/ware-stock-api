using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Products;

public class ProductDto
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

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.Created))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.LastModified));
        }
    }
}
