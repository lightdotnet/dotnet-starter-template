using Microsoft.AspNetCore.Mvc;

namespace Monolith.Controllers;

public class HelloController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
