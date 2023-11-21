namespace HuertaFacilApi.Models;

public class Bonificadores
{
    
    public int PlantaId { get; set;}
    public int BiopreparadoId { get; set;}

    public Biopreparados? Biopreparado {get;set;}

}