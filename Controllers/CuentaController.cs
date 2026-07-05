using System.Security.Claims;
using GestionUsuarios.Data;
using GestionUsuarios.Models;
using GestionUsuarios.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Controllers;

public class CuentaController : Controller
{
    private readonly AuthService _authService;
    private readonly AppDbContext _db;
    private readonly ILogger<CuentaController> _logger;

    public CuentaController(AuthService authService, AppDbContext db, ILogger<CuentaController> logger)
    {
        _authService = authService;
        _db = db;
        _logger = logger;
    }

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
                model.ErrorContrasena = resultado.IntentosRestantes > 0
                    ? $"Contraseña incorrecta. Intentos restantes: {resultado.IntentosRestantes}"
                    : "Contraseña incorrecta";
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
    public IActionResult Bloqueada(int minutos = 15, bool soporte = false)
    {
        ViewBag.Minutos = minutos;
        ViewBag.SoporteNotificado = soporte;
        return View();
    }

    [HttpGet]
    public IActionResult Activacion() => View(new ActivacionViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activacion(ActivacionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u =>
            u.TipoDocumento == model.TipoDocumento &&
            u.NumeroDocumento == model.NumeroDocumento.Trim() &&
            u.Activo);

        if (usuario is null)
        {
            model.MensajeError = "No se encontró una cuenta activa con ese documento.";
            return View(model);
        }

        model.Activada = true;
        model.Nombre = usuario.Nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? usuario.Nombres;
        _logger.LogInformation("Cuenta activada en flujo demo para {TipoDocumento}-{NumeroDocumento}",
            usuario.TipoDocumento, usuario.NumeroDocumento);
        return View(model);
    }

    [HttpGet]
    public IActionResult RecuperarContrasena() => View(new RecuperarContrasenaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarContrasena(RecuperarContrasenaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u =>
            u.TipoDocumento == model.TipoDocumento &&
            u.NumeroDocumento == model.NumeroDocumento.Trim() &&
            u.CorreoPrincipal == model.Correo.Trim() &&
            u.Activo);

        if (usuario is null)
        {
            model.MensajeError = "Los datos ingresados no coinciden con una cuenta registrada.";
            return View(model);
        }

        model.Enviado = true;
        _logger.LogInformation("[CORREO SIMULADO] Para: {Correo} - Asunto: Recuperación de contraseña.",
            usuario.CorreoPrincipal);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NotificarSoporte()
    {
        _logger.LogWarning("[SOPORTE SIMULADO] Solicitud de revisión de cuenta bloqueada enviada a control técnico.");
        return RedirectToAction(nameof(Bloqueada), new { soporte = true });
    }

    /// <summary>Mantiene viva la sesión cuando el usuario pulsa "Extender sesión".</summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult ExtenderSesion() => Ok(new { ok = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(bool expirada = false)
    {
        return await CerrarSesion(expirada);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        return await CerrarSesion();
    }

    private async Task<IActionResult> CerrarSesion(bool expirada = false)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (expirada)
            TempData["SesionExpirada"] = "Su sesión ha expirado debido a inactividad. Por favor, inicie sesión nuevamente.";

        return RedirectToAction(nameof(Login));
    }
}
