using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Monolith.Blazor.Extensions;
using Monolith.Blazor.Services;
using Monolith.HttpApi.Identity;
using System.Security.Claims;
using Light.Identity;

namespace Monolith.Blazor.Components.Account
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/account");

            accountGroup.MapGet("/post-login", async (
                HttpContext httpContext,
                string? returnUrl) =>
            {
                var tokenService = httpContext.RequestServices.GetRequiredService<TokenHttpService>();

                var getToken = await tokenService.GetTokenAsync("super", "123");

                var id = Guid.NewGuid().ToString("N");

                var tokenData = new TokenModel(
                    getToken.Data.AccessToken,
                    getToken.Data.ExpiresIn,
                    getToken.Data.RefreshToken);

                var userClaims = JwtExtensions.ReadClaims(getToken.Data.AccessToken);

                var userProfileService = httpContext.RequestServices.GetRequiredService<UserProfileHttpService>();

                var getUserProfiles = await userProfileService.GetAsync();

                if (getUserProfiles.Succeeded)
                {
                    userClaims.AddRange(getUserProfiles.Data.Get());
                }

                var claimsIdentity = new ClaimsIdentity(userClaims, Constants.JwtAuthScheme);

                // Replace with new ClaimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in using Identity's scheme
                await httpContext.SignInAsync(
                    Constants.JwtAuthScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,  // "Remember me"
                        ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(getToken.Data.ExpiresIn),
                        AllowRefresh = true
                    });

                return Results.Ok();
            }).AllowAnonymous();

            accountGroup.MapGet("/logout", async (
                [FromServices] ISignInManager service,
                [FromQuery] string? returnUrl) =>
            {
                await service.SignOutAsync();

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            accountGroup.MapPost("/post-logout", async (
                [FromServices] ISignInManager service,
                [FromForm] string? returnUrl) =>
            {
                await service.SignOutAsync();

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            return accountGroup;
        }
    }
}
