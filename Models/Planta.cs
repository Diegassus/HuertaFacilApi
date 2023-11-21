namespace HuertaFacilApi.Models;

public class Planta {
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
    public required int TipoId { get; set; }
    public Tipo_Planta? Tipo { get; set; }
    public required int LuzId { get; set; }
    public Luz? Iluminacion { get; set; }
}