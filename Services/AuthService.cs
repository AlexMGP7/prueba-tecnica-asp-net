using GestionUsuarios.Data;
using GestionUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Services;

public enum ResultadoLogin
{
    Ok,
    UsuarioIncorrecto,
    ContrasenaIncorrecta,
    CuentaBloqueada
}

public record LoginResultado(
    ResultadoLogin Resultado,
    Usuario? Usuario = null,
    int MinutosBloqueoRestantes = 0,
    int IntentosRestantes = 0);

/// <summary>
/// Valida credenciales aplicando el flujo del diseño:
/// - Cuenta bloqueada → rechaza sin evaluar credenciales.
/// - Usuario inexistente → "Usuario incorrecto".
/// - Contraseña errada → incrementa el CVF (Contador de Validaciones Fallidas);
///   al 5.º fallo bloquea la cuenta 15 minutos y notifica por correo (simulado).
/// - Éxito → reinicia el CVF.
/// </summary>
public class AuthService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuthService> _logger;
    private readonly int _maxIntentos;
    private readonly int _minutosBloqueo;

    public AuthService(AppDbContext db, IConfiguration config, ILogger<AuthService> logger)
    {
        _db = db;
        _logger = logger;
        _maxIntentos = config.GetValue("Seguridad:MaxIntentosFallidos", 5);
        _minutosBloqueo = config.GetValue("Seguridad:MinutosBloqueo", 15);
    }

    public async Task<LoginResultado> ValidarAsync(string tipoDocumento, string numeroDocumento, string contrasena)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u =>
            u.TipoDocumento == tipoDocumento && u.NumeroDocumento == numeroDocumento && u.Activo);

        if (usuario is null)
            return new LoginResultado(ResultadoLogin.UsuarioIncorrecto);

        if (EstaBloqueada(usuario))
        {
            var restante = (int)Math.Ceiling((usuario.BloqueadaHasta!.Value - DateTime.UtcNow).TotalMinutes);
            return new LoginResultado(ResultadoLogin.CuentaBloqueada, usuario, restante);
        }

        if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= _maxIntentos)
            {
                usuario.BloqueadaHasta = DateTime.UtcNow.AddMinutes(_minutosBloqueo);
                usuario.IntentosFallidos = 0;
                await _db.SaveChangesAsync();
                NotificarBloqueoPorCorreo(usuario);
                return new LoginResultado(ResultadoLogin.CuentaBloqueada, usuario, _minutosBloqueo);
            }

            await _db.SaveChangesAsync();
            var intentosRestantes = Math.Max(_maxIntentos - usuario.IntentosFallidos, 0);
            return new LoginResultado(ResultadoLogin.ContrasenaIncorrecta, usuario, IntentosRestantes: intentosRestantes);
        }

        // Credenciales válidas: se reinicia el contador y se limpia cualquier bloqueo vencido
        usuario.IntentosFallidos = 0;
        usuario.BloqueadaHasta = null;
        await _db.SaveChangesAsync();
        return new LoginResultado(ResultadoLogin.Ok, usuario);
    }

    private static bool EstaBloqueada(Usuario usuario) =>
        usuario.BloqueadaHasta.HasValue && usuario.BloqueadaHasta.Value > DateTime.UtcNow;

    /// <summary>
    /// N2 del diseño: "Correo de notificación de cuenta bloqueada".
    /// En un entorno real se integraría un servicio SMTP; aquí se registra en el log.
    /// </summary>
    private void NotificarBloqueoPorCorreo(Usuario usuario)
    {
        _logger.LogWarning(
            "[CORREO SIMULADO] Para: {Correo} — Asunto: Cuenta bloqueada temporalmente. " +
            "Su cuenta fue bloqueada {Minutos} minutos por exceder {Max} intentos fallidos.",
            usuario.CorreoPrincipal, _minutosBloqueo, _maxIntentos);
    }
}
