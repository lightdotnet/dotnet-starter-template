namespace Monolith;

public abstract class EntityBase : Light.Domain.Entities.AuditableEntity;

public abstract class EntityBase<T> : Light.Domain.Entities.BaseAuditableEntity<T>;
