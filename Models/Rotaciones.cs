using System.ComponentModel.DataAnnotations.Schema;

namespace HuertaFacilApi.Models;

public class Rotaciones 
{
    public int Anterior {get;set;}
    public int Posterior {get;set;}
    [ForeignKey("Posterior")]
    public Planta? Planta {get;set;}
}