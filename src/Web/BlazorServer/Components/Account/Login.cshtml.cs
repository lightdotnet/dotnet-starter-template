using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monolith.Infrastructure;

namespace Monolith.Components.Account
{
    public class LoginModel(IAuthService authService) : PageModel
    {
        public void OnGet()
        {
        }

        public async Task OnPost()
        {
            var login = await authService.LoginAsync("super", "123", true);

            if (login.Succeeded)
            {
                Redirect("/");
            }
        }
    }
}
