using Microsoft.AspNetCore.Mvc;

namespace SPOTIFY.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
