using Light.Contracts;
using System.Security.Claims;

namespace Monolith.Core;

public interface IIdentityManager
{
    ClaimsPrincipal? CurrentUser { get; }

    Task<Result> LoginAsync(string userName, string password);

    Task LogoutAsync();
}
