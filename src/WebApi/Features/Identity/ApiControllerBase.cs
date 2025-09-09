using Light.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Monolith.Features.Identity;

[ApiExplorerSettings(GroupName = "Admin")]
public abstract class ApiControllerBase : VersionedApiController;
