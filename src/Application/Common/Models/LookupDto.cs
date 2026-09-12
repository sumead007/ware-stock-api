using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Common.Models;

/// <summary>Generic `{ value, label }` pair used for creatable comboboxes (product category/unit).</summary>
public class LookupDto
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            // Value must match Product.Category/Product.Unit, which store the label text
            // itself (not the lookup entity's surrogate id) — so Value mirrors Label here.
            CreateMap<ProductCategory, LookupDto>()
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Label))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));

            CreateMap<ProductUnit, LookupDto>()
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Label))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));
        }
    }
}
