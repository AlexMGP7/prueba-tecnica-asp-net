using System.Security.Claims;
using GestionUsuarios.Models;
using GestionUsuarios.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionUsuarios.Controllers;

public class CuentaController : Controller
{
    private readonly AuthService _authService;

    public CuentaController(AuthService authService) => _authService = authService;

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Perfil");

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _authService.ValidarAsync(model.TipoDocumento, model.Usuario.Trim(), model.Contrasena);

        switch (resultado.Resultado)
        {
            case ResultadoLogin.CuentaBloqueada:
                return RedirectToAction(nameof(Bloqueada), new { minutos = resultado.MinutosBloqueoRestantes });

            case ResultadoLogin.UsuarioIncorrecto:
                model.ErrorUsuario = "Usuario incorrecto";
                model.ErrorContrasena = "Contraseña incorrecta";
                return View(model);

            case ResultadoLogin.ContrasenaIncorrecta:
                model.ErrorContrasena = "Contraseña incorrecta";
                return View(model);
        }

        var usuario = resultado.Usuario!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties { IsPersistent = false });

        return RedirectToAction("Index", "Perfil");
    }

    [HttpGet]
    public IActionResult Bloqueada(int minutos = 15)
    {
        ViewBag.Minutos = minutos;
        return View();
    }

    [HttpGet]
    public IActionResult Activacion() => View();

    /// <summary>Mantiene viva la sesión cuando el usuario pulsa "Extender sesión".</summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult ExtenderSesion() => Ok(new { ok = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(bool expirada = false)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (expirada)
            TempData["SesionExpirada"] = "Su sesión ha expirado debido a inactividad. Por favor, inicie sesión nuevamente.";

        return RedirectToAction(nameof(Login));
    }
}
