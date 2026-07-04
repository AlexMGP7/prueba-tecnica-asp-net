using GestionUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasIndex(u => new { u.TipoDocumento, u.NumeroDocumento }).IsUnique();
        });
    }
}
