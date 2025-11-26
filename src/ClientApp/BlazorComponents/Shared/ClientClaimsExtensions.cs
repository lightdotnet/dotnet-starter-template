using Light.Extensions;
using Monolith.Authorization;
using System.Reflection;

namespace Monolith.Blazor.Shared;

public class ClientClaimsExtensions
{
    public static AppClaimsVm GetAll()
    {
        var fromClass = typeof(Permissions);

        var result = new AppClaimsVm();

        // get classes in class
        var modules = fromClass.GetNestedTypes();

        foreach (var module in modules)
        {
            var group = new ClaimGroupDto(module.GetDisplayName() ?? module.Name, module.GetDescription());
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
                    result.Claims.Add(new AppClaimDto(
                        group.Name,
                        propertyValue,
                        fi.GetDisplayName() ?? fi.Name,
                        fi.GetNameOfDisplay()));
                }
                //TODO - take descriptions from description attribute
            }
        }

        return result;
    }
}
