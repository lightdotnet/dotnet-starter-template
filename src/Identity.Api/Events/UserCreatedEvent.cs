using StarterKit.Shared.Entities;

namespace StarterKit.Identity.Api.Events;

public record UserCreatedEvent(
    string UserId,
    string? UserName,
    string? Email)
    : DomainEvent;
