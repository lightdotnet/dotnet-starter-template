using Light.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monolith.Blazor.Extensions;
using Monolith.Blazor.Services;
using Monolith.HttpApi.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Monolith.Blazor.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    [BindProperty]
    [Required]
    public string UserName { get; set; } = default!;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [BindProperty]
    public bool RememberMe { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var tokenService = HttpContext.RequestServices.GetRequiredService<TokenHttpService>();

        var getToken = await tokenService.GetTokenAsync(UserName, Password);

        if (getToken.Succeeded is false)
        {
            ModelState.AddModelError("", getToken.Message);

            return Page();
        }

        var id = Guid.NewGuid().ToString("N");

        var tokenData = new TokenModel(
            getToken.Data.AccessToken,
            getToken.Data.ExpiresIn,
            getToken.Data.RefreshToken);

        var userClaims = JwtExtensions.ReadClaims(getToken.Data.AccessToken);

        var userProfileService = HttpContext.RequestServices.GetRequiredService<UserProfileHttpService>();

        var getUserProfiles = await userProfileService.GetAsync();

        if (getUserProfiles.Succeeded)
        {
            userClaims.AddRange(getUserProfiles.Data.Get());
        }

        var claimsIdentity = new ClaimsIdentity(userClaims, Constants.JwtAuthScheme);

        // Replace with new ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Sign in using Identity's scheme
        await HttpContext.SignInAsync(
            Constants.JwtAuthScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = RememberMe,  // "Remember me"
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(getToken.Data.ExpiresIn),
                AllowRefresh = true
            });

        return LocalRedirect("/");
    }
}
