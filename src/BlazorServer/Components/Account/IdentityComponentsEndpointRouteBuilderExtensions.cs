using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                [FromServices] AuthenticationStateProvider authProvider,
                [FromQuery] string returnUrl) =>
            {
                if (authProvider is JwtAuthStateProvider state)
                {
                    await state.LogoutAsync();
                }

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            accountGroup.MapPost("/post-logout", async (
                [FromServices] AuthenticationStateProvider authProvider,
                [FromForm] string returnUrl) =>
            {
                if (authProvider is JwtAuthStateProvider state)
                {
                    await state.LogoutAsync();
                }

                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            return accountGroup;
        }
    }
}
