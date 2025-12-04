using Microsoft.AspNetCore.Mvc;
using Monolith.Blazor.Services;

namespace Monolith.Blazor.Components.Account
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/account");

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
