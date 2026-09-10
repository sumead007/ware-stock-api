using System.ComponentModel.DataAnnotations.Schema;

namespace WareStockApi.Domain.Common;

public abstract class BaseEntity
{
    // string id (GUID) - matches the OpenAPI spec, where every entity's `id` is a string.
    public string Id { get; set; } = Guid.NewGuid().ToString();

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
