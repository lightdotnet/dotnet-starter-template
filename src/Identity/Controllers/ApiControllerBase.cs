using Light.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Monolith.Identity.Controllers;

[ApiExplorerSettings(GroupName = "Admin")]
public abstract class ApiControllerBase : VersionedApiController;
