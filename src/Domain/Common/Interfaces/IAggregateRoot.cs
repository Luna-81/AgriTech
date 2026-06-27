namespace Domain.Common.Interfaces;

public interface IAggregateRoot
{
    IReadOnlyCollection<object>? DomainEvents { get; }
    void ClearDomainEvents();
}