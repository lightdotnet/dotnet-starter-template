namespace Monolith.WebAdmin.Services.Storage;

public class HttpContextEntensions
{
    public static void ClearCookies(HttpContext httpContext)
    {
        // clear all cookies
        foreach (var cookie in httpContext.Request.Cookies.Keys)
        {
            httpContext.Response.Cookies.Delete(cookie);
        }
    }
}
