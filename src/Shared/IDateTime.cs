namespace StarterKit;

public interface IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset AuditTime => DateTimeOffset.UtcNow;
}
