using Microsoft.EntityFrameworkCore;

namespace HuertaFacilApi.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options){}
    public DbSet<Usuario> Usuarios { get; set; } = null!;
}