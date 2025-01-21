using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class FallbackController : Controller
{
    public ActionResult Index()
    {
        var result = PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        return result;
    }
}
