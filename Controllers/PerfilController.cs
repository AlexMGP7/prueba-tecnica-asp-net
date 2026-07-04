using System.Security.Claims;
using GestionUsuarios.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly AppDbContext _db;

    public PerfilController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null)
            return RedirectToAction("Login", "Cuenta");

        return View(usuario);
    }
}
