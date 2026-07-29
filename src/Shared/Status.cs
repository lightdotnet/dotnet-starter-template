using Light.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace StarterKit;

public class Status : ValueObject
{
    public Status() { }

    public Status(ActiveStatus status)
    {
        Value = status;
    }

    public enum ActiveStatus
    {
        Inactive = 0,
        Active = 1,
        Locked = 2,
    }

    public ActiveStatus Value { get; set; } = ActiveStatus.Active;

    [JsonIgnore]
    public bool IsInactive => Value == ActiveStatus.Inactive;

    [JsonIgnore]
    public bool IsActive => Value == ActiveStatus.Active;

    [JsonIgnore]
    public bool IsLocked => Value == ActiveStatus.Locked;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public void Update(ActiveStatus status)
    {
        Value = status;
    }
}
