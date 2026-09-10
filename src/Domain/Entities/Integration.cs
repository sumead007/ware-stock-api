namespace WareStockApi.Domain.Entities;

/// <summary>
/// Read-only, seeded list of connectable apps shown on the "Apps" page. <see cref="BaseEntity.Id"/>
/// is the app's slug (e.g. "github", "slack") rather than a random GUID.
/// </summary>
public class Integration : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Desc { get; set; } = string.Empty;

    public bool Connected { get; set; }
}
