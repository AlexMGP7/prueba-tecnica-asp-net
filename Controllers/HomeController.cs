using System.Diagnostics;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionUsuarios.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Login", "Cuenta");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
