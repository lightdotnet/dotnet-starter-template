using Light.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Monolith;

public class Status : ValueObject
{
    public Status() { }

    public Status(ActiveStatus status)
    {
        Value = status;
    }

    public enum ActiveStatus
    {
        unactive = 0,
        active = 1,
        locked = 2,
    }

    public ActiveStatus Value { get; set; } = ActiveStatus.active;

    [JsonIgnore]
    public bool IsUnactive => Value == ActiveStatus.unactive;

    [JsonIgnore]
    public bool IsActive => Value == ActiveStatus.active;

    [JsonIgnore]
    public bool IsLocked => Value == ActiveStatus.locked;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Using a yield return statement to return each element one at a time
        yield return Value;
        yield return IsUnactive;
        yield return IsActive;
        yield return IsLocked;
    }

    public void Update(ActiveStatus status)
    {
        Value = status;
    }
}