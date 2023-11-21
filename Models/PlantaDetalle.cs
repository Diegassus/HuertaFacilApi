namespace HuertaFacilApi.Models;

public class PlantaDetalle {
    public int Id { get; set; }
    public required string Logo { get; set; }
    public required string Portada { get; set; }
    public required string Nombre { get; set; }
    public required int Mes { get; set; }
    public required int Transplante { get; set; }
    public required int Riego { get; set; }
    public required int Cosecha { get; set; }
    public required int Semillado { get; set; }
    public required int Poda { get; set; }
    public Tipo_Planta? Tipo { get; set; }
    public Luz? Iluminacion { get; set; }

    public PlantaDetalle(int id, string logo, string portada, string nombre, int mes, int transplante, int riego, int cosecha, int semillado, int poda, Tipo_Planta categoria, Luz iluminacion)
    {
        this.Id = id;
        this.Logo = logo;
        this.Portada = portada;
        this.Nombre = nombre;
        this.Mes = mes;
        this.Transplante = transplante;
        this.Riego = riego;
        this.Cosecha = cosecha;
        this.Semillado = semillado;
        this.Poda = poda;
        this.Tipo = categoria;
        this.Iluminacion = iluminacion;
    }
}

