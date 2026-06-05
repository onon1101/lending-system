using LendingSystem.SharedKernel.Domain.Abstractions;

namespace LendingSystem.SharedKernel.Domain.Common;

public abstract class Entity
{
    private List<IDomainEvent>? _domainEvents;
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly() ?? [];
    
    public void ClearDomainEvents() => _domainEvents?.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];
        
        _domainEvents.Add(domainEvent);
    }

}
