using Microsoft.AspNetCore.Mvc;
using Monolith.BlazorServer.Services;

namespace Monolith.BlazorServer.Components.Account
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/account");

            accountGroup.MapGet("/logout", async (
                [FromServices] IAuthService authService,
                [FromQuery] string? returnUrl) =>
            {
                await authService.LogoutAsync();

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            accountGroup.MapPost("/post-logout", async (
                [FromServices] IAuthService authService,
                [FromForm] string? returnUrl) =>
            {
                await authService.LogoutAsync();

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            return accountGroup;
        }
    }
}
