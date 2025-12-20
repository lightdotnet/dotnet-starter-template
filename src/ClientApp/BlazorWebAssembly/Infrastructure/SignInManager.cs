using Monolith.Blazor.Shared;

namespace Monolith.Blazor.Infrastructure;

public interface ISignInManager
{
    Task<Result> SignInAsync(LoginRequest model);

    Task SignOutAsync();
}
