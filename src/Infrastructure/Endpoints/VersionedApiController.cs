using Asp.Versioning;

namespace Monolith.Endpoints;

[ApiVersion("1.0")]
public abstract class VersionedApiController : Light.AspNetCore.Mvc.VersionedApiController;
