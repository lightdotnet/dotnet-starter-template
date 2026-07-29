using StarterKit.Entities;

namespace StarterKit.Identity.Events;

public record UserCreatedEvent(
    string UserId,
    string? UserName,
    string? Email)
    : DomainEvent;
