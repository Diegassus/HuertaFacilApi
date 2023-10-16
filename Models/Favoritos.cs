using System.ComponentModel.DataAnnotations.Schema;

namespace HuertaFacilApi.Models;

public class Favoritos
{
    public int Id { get; set; }
    public int UsuarioId { get; set;}
    public int PlantaId { get; set;}
    public Planta? Planta { get; set;}

}