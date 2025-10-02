using System.Reflection;
using System.Security.Claims;

namespace Monolith;

public abstract class PermissionList
{
    private const string _permissionClaimType = Light.Identity.ClaimTypes.Permission;

    public static IEnumerable<Claim> All
    {
        get
        {
            var fromClass = typeof(Permissions);

            var claims = new List<Claim>();

            // get classes in class
            var modules = fromClass.GetNestedTypes();

            foreach (var module in modules)
            {
                // get props in class
                var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                foreach (FieldInfo fi in fields)
                {
                    var propertyValue = fi.GetValue(null)?.ToString();

                    if (propertyValue != null)
                    {
                        claims.Add(new Claim(_permissionClaimType, propertyValue));
                    }
                    //TODO - take descriptions from description attribute
                }
            }

            return claims;
        }
    }

    public static bool IsPermissionValid(string permission) =>
        All.Any(x => x.Type == _permissionClaimType && x.Value == permission);
}