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
            CreateMap<ProductCategory, LookupDto>()
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));

            CreateMap<ProductUnit, LookupDto>()
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));
        }
    }
}
