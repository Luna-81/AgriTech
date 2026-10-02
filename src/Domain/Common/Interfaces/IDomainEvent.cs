namespace Domain.Common.Interfaces;

/// <summary>
/// Represents a domain event that occurs within the domain model.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}