using Microsoft.EntityFrameworkCore;

namespace HuertaFacilApi.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options){}
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Planta> Plantas { get ; set ;} = null! ;
    public DbSet<Favoritos> Favoritos { get; set; } = null!;
    public DbSet<Rotaciones> Rotaciones {get;set;} = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favoritos>().HasKey(f => new { f.PlantaId, f.UsuarioId });
        modelBuilder.Entity<Bonificadores>().HasKey(f => new { f.PlantaId, f.BiopreparadoId });
        modelBuilder.Entity<Ataques>().HasKey(f => new { f.PlantaId, f.AmenazaId });
        modelBuilder.Entity<Contrarias>().HasKey(f => new { f.PlantaId, f.Contraria });
        modelBuilder.Entity<Rotaciones>().HasKey(p => new { p.Anterior, p.Posterior});
        modelBuilder.Entity<Curas>().HasKey(p => new {p.AmenazaId, p.BiopreparadoId});
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Tipo_Planta> Tipo_planta {get; set;} = null!;
    public DbSet<Contrarias> Contrarias {get;set;} = null!;
    public DbSet<Bonificadores> Bonificadores {get;set;} = null!;
    public DbSet<Amenazas> Amenazas {get;set;} = null!;
    public DbSet<Curas> Curas {get;set;} = null!;
    public DbSet<Ataques> Ataques {get;set;} = null!;
    public DbSet<Biopreparados> Biopreparados {get;set;} = null!;
    public DbSet<Luz> Luces {get;set;} = null!;
    public DbSet<Tips> Tips { get; set; } = null!;
    public DbSet<Usos> Usos {get;set;} =null! ;
    public DbSet<Documentos> Documentos {get;set;} = null!;
    public DbSet<Recordatorios> Recordatorios {get;set;} = null!;
    public DbSet<Tipo_Recordatorio> Tipo_Recordatorio {get;set;} = null!;
}