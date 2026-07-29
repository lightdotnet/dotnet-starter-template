using Light.Domain;

namespace Monolith;

public abstract class AuditableEntity<T> : Light.Domain.Entities.BaseAuditableEntity<T>;

public abstract class AuditableEntity : AuditableEntity<string>
{
    protected AuditableEntity() => Id = LightId.NewId();
}

public abstract record DomainEvent : Light.Domain.Entities.BaseEvent, Light.Mediator.INotification;
