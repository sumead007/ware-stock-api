using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Integrations;

public class IntegrationDto
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Desc { get; init; } = string.Empty;

    public bool Connected { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Integration, IntegrationDto>();
        }
    }
}
