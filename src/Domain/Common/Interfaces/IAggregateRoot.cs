namespace Domain.Common.Interfaces;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent>? DomainEvents { get; }
    void ClearDomainEvents();
}