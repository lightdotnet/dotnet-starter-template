using Microsoft.AspNetCore.Mvc.Razor;

namespace Monolith.Mvc.Extensions;

public class ViewLocationExpander : IViewLocationExpander
{
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        return
        [
            "~/Areas/{2}/Views/{1}/{0}.cshtml", // default Areas

            "~/Views/{1}/{0}.cshtml", // default MVC
            "~/Views/Shared/{0}.cshtml", // default MVC

            "~/Pages/Shared/{0}.cshtml", // default Razor Pages

            "~/{1}/Views/{0}.cshtml", // custom views at root/[features]/views

            "~/Modules/{1}/Views/{0}.cshtml", // custom views at root/Modules/[module-names]/[features]/views
        ];
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // Nothing needs to be done here
    }
}