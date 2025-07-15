namespace Monolith.Domain.Shops;

public class Shop : EntityBase
{
    public string Name { get; set; } = null!;

    public Status Status { get; set; } = new(Status.ActiveStatus.active);

    public static Shop Create(string name)
    {
        return new Shop { Name = name };
    }
}
