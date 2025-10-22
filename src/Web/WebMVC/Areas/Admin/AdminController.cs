using Microsoft.AspNetCore.Mvc;

namespace Monolith.Areas.Admin;

[Area("Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
