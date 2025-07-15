using Light.Contracts;
using Light.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Components.Pages.Account;

public class LoginService(
    UserManager<User> userManager,
    SignInManager<User> signInManager)
{
    public async Task<Result> LoginAsync(string username, string password, bool rememberMe)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
            return Result.Error();

        var login = await userManager.CheckPasswordAsync(user, password);

        if (login)
        {
            await signInManager.SignInAsync(user, rememberMe);

            return Result.Success();
        }
        else
        {
            return Result.Error();
        }
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }
}
