using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models;

public class ActivacionViewModel
{
    [Required(ErrorMessage = "Seleccione el tipo de documento")]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "Ingrese el número de documento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    public bool Activada { get; set; }

    public string Nombre { get; set; } = "July";

    public string? MensajeError { get; set; }
}
