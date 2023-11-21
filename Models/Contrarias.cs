using System.ComponentModel.DataAnnotations.Schema;

namespace HuertaFacilApi.Models;

public class Contrarias
{
    
    public int PlantaId {get;set;}
    public int Contraria {get;set;}
    [ForeignKey("Contraria")]
    public Planta? Planta {get;set;}

}