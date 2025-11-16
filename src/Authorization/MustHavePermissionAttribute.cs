using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string policy) => Policy = policy;
}