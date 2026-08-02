using Microsoft.AspNetCore.Mvc;
using StarterKit.Infrastructure.Endpoints;

namespace StarterKit.Identity.Api.Controllers;

[ApiExplorerSettings(GroupName = "identity")]
public class PermissionsController(
    IPermissionDefinitionProvider permissionDefinitionProvider) : VersionedApiController
{
    [HttpGet]
    public IActionResult GetAsync()
    {
        var permissions = permissionDefinitionProvider.Define();
        return Ok(permissions);
    }
}
