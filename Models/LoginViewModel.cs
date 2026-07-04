using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models;

public class LoginViewModel
{
    [Required]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "Ingrese su usuario")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    // Errores inline específicos por campo, según el flujo del diseño
    public string? ErrorUsuario { get; set; }
    public string? ErrorContrasena { get; set; }
}
