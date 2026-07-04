using GestionUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Data;

public static class DbSeeder
{
    /// <summary>
    /// Crea la base de datos si no existe y siembra los usuarios de prueba.
    /// Reintenta varias veces porque el contenedor de SQL Server tarda unos
    /// segundos en aceptar conexiones tras arrancar.
    /// </summary>
    public static void Seed(AppDbContext db, ILogger logger)
    {
        const int maxIntentos = 12;
        for (var intento = 1; ; intento++)
        {
            try
            {
                db.Database.EnsureCreated();
                break;
            }
            catch (Exception ex) when (intento < maxIntentos)
            {
                logger.LogWarning("SQL Server aún no disponible (intento {Intento}/{Max}): {Mensaje}",
                    intento, maxIntentos, ex.Message);
                Thread.Sleep(5000);
            }
        }

        if (db.Usuarios.Any()) return;

        db.Usuarios.AddRange(
            new Usuario
            {
                TipoDocumento = "DNI",
                NumeroDocumento = "07079879",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ceplan#2026"),
                Nombres = "July Camila",
                PrimerApellido = "Mendoza",
                SegundoApellido = "Quispe",
                FechaNacimiento = new DateTime(1944, 4, 1),
                Nacionalidad = "Peruana",
                Sexo = "Femenino",
                CorreoPrincipal = "test@minsa.gob.pe",
                TelefonoMovil = "+51 999 999 999",
                TipoContratacion = "CAS",
                FechaContratacion = new DateTime(2015, 3, 9),
                Cargo = "Administrador de Recursos",
                Entidad = "011 Ministerio de Salud",
                Rol = "Administrador de Recursos",
                Activo = true
            },
            new Usuario
            {
                TipoDocumento = "CE",
                NumeroDocumento = "001234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ceplan#2026"),
                Nombres = "Adriana",
                PrimerApellido = "Osorio",
                SegundoApellido = "Montes",
                FechaNacimiento = new DateTime(1988, 6, 15),
                Nacionalidad = "Colombiana",
                Sexo = "Femenino",
                CorreoPrincipal = "aosorio@ceplan.gob.pe",
                TelefonoMovil = "+51 988 888 888",
                TipoContratacion = "CAS",
                FechaContratacion = new DateTime(2019, 8, 1),
                Cargo = "Operador",
                Entidad = "016 CEPLAN",
                Rol = "Operador",
                Activo = true
            });

        db.SaveChanges();
        logger.LogInformation("Base de datos creada y usuarios de prueba sembrados.");
    }
}
