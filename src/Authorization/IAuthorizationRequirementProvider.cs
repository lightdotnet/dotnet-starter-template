using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public interface IAuthorizationRequirementProvider
{
    IAuthorizationRequirement[] Requirements { get; }
}
