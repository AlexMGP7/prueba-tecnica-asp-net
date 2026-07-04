using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, MaxLength(3)]
    public string TipoDocumento { get; set; } = "DNI"; // DNI | CE

    [Required, MaxLength(12)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string PrimerApellido { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string SegundoApellido { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    [MaxLength(40)]
    public string Nacionalidad { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Sexo { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string CorreoPrincipal { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? CorreoSecundario { get; set; }

    [MaxLength(20)]
    public string TelefonoMovil { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TelefonoSecundario { get; set; }

    [MaxLength(30)]
    public string TipoContratacion { get; set; } = string.Empty;

    public DateTime FechaContratacion { get; set; }

    [MaxLength(80)]
    public string Cargo { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Entidad { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    /// <summary>CVF: Contador de Validaciones Fallidas.</summary>
    public int IntentosFallidos { get; set; }

    /// <summary>Si tiene valor y es futuro, la cuenta está bloqueada temporalmente.</summary>
    public DateTime? BloqueadaHasta { get; set; }

    public string NombreCompleto => $"{PrimerApellido} {SegundoApellido}, {Nombres}";
}
