namespace HuertaFacilApi.Models;

public class Ataques{
    public int PlantaId {get;set;}
    public int AmenazaId {get;set;}
    public Amenazas? Amenaza {get;set;}
}