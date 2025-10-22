using Light.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Monolith.Areas.Identity;

public class RoleController(IRoleService roleService) : ControllerBase
{
    public async Task<IActionResult> Index()
    {
        return View(await roleService.GetAllAsync());
    }
}
