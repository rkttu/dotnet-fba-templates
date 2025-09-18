using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Message"] = "Minimal API and MVC controllers working together!";
        return View();
    }

    public IActionResult About()
        => Content("This page was rendered from the Home/About action.");
}
