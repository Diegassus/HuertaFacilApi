namespace HuertaFacilApi.Models;

public class PlantaListado {
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Foto { get; set; }
    public PlantaListado(int id, string nombre, string foto){
        this.Id = id;
        this.Nombre = nombre;
        this.Foto = foto;
    }
}