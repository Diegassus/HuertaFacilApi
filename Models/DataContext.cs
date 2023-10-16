using Microsoft.EntityFrameworkCore;

namespace HuertaFacilApi.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options){}
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Planta> Plantas { get ; set ;} = null! ;
    public DbSet<Favoritos> Favoritos { get; set; } = null!;
    public DbSet<Tips> Tips { get; set; } = null!;
}