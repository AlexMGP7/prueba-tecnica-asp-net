using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models;

public class RecuperarContrasenaViewModel
{
    [Required(ErrorMessage = "Seleccione el tipo de documento")]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "Ingrese el número de documento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el correo registrado")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
    public string Correo { get; set; } = string.Empty;

    public bool Enviado { get; set; }

    public string? MensajeError { get; set; }
}
