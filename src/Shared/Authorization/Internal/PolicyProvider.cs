using Light.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Monolith.Authorization.Internal;

internal class PolicyProvider(IOptions<AuthorizationOptions> options) : PermissionPolicyProvider(options)
{ }