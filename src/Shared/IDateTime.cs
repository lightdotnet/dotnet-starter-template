namespace Monolith;

public interface IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
