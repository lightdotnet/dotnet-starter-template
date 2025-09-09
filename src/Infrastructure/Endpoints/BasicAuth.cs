using Light.AspNetCore.Authorization;
using Light.AspNetCore.Mvc;
using Light.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith.Endpoints;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class BasicAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var key = configuration.GetValue<string>("BasicAuth");

        var authFromRequest = context.HttpContext.Request.ReadBasicAuthorization();

        var isRequestValid = authFromRequest.Equals(key);

        if (!isRequestValid)
        {
            context.Result = Result.Unauthorized().ToActionResult();
            return;
        }
    }
}
