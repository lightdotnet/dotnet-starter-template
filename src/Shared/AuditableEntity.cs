namespace Monolith;

public abstract class AuditableEntity : Light.Domain.Entities.Default.AuditableEntity;

public abstract class AuditableEntity<T> : Light.Domain.Entities.AuditableEntity<T>;
