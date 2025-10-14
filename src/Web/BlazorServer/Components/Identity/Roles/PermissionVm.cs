using Light.Extensions;
using System.Reflection;

namespace Monolith.Components.Identity.Roles;

public class PermissionVm
{
    public List<PermissionGroup> Groups { get; set; } = [];

    public List<PermissionValue> Values { get; set; } = [];

    public static PermissionVm LoadPermissions(IEnumerable<string> owned)
    {
        var fromClass = typeof(Permissions);

        var result = new PermissionVm();

        // get classes in class
        var modules = fromClass.GetNestedTypes();

        foreach (var module in modules)
        {
            var group = new PermissionGroup(module.GetDisplayName() ?? module.Name, module.GetDescription());
            result.Groups.Add(group);

            // get props in class
            var fields = module
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => x != null);

            foreach (FieldInfo fi in fields)
            {
                if (fi == null)
                    continue;

                var propertyValue = fi.GetValue(null)?.ToString();

                if (propertyValue != null)
                {
                    var permission = new PermissionValue(
                        group.Name,
                        propertyValue,
                        fi.GetDisplayName() ?? fi.Name,
                        fi.GetNameOfDisplay());

                    if (owned.Contains(permission.Value))
                    {
                        permission.IsOwned = true;
                    }

                    result.Values.Add(permission);
                }
                //TODO - take descriptions from description attribute
            }
        }

        return result;
    }
}

public record PermissionGroup(string Name, string? Description)
{
    public override string ToString()
    {
        var displayName = Name;
        if (!string.IsNullOrEmpty(Description))
        {
            displayName += $" ({Description})";
        }

        return displayName;
    }
}

public record PermissionValue(string GroupName, string Value, string Name, string? Description)
{
    public bool IsOwned { get; set; }

    public override string ToString()
    {
        var displayName = Name;
        if (!string.IsNullOrEmpty(Description))
        {
            displayName += $" ({Description})";
        }

        return displayName;
    }
}