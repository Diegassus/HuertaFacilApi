namespace HuertaFacilApi.Models;

public class Usuario{
    public int Id { get; set; }
    public string? Correo { get; set; }
    public string? Clave { get; set; }
    public bool? Hemisferio { get; set; }
    public bool Administrador { get; set; }
    public DateTime? Alta { get; set; }
    
}